using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private InputAction? FindHintAction(string mapName, string actionName)
    {
        InputRebindingManager? manager = _controlsRebindingManager;
        if (manager == null)
        {
            MainMenuUIManager? main = _mainMenu;
            if (main == null)
                main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            manager = main?.controlsPanel?
                .GetComponentInChildren<InputRebindingManager>(true);
        }

        // The PlayerInput copy has the player's active binding overrides.
        return manager?.playerInput?.actions?
            .FindActionMap(mapName, false)?.FindAction(actionName, false) ??
            manager?.inputActions?
                .FindActionMap(mapName, false)?.FindAction(actionName, false);
    }

    private static InputAction? HintUiMoveAction()
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        return module?.move?.action;
    }

    private string HintNativeAction(string name, string purpose,
        string keyboardFallback, string controllerFallback,
        bool controllerIsAxis = false) =>
        HintActionSentence(FindHintAction("Gameplay", name), null, purpose,
            keyboardFallback, controllerFallback, null,
            controllerIsAxis ? "axis" : "press");

    private static string HintActionSentence(InputAction? preferred,
        InputAction? secondary, string purpose, string keyboardFallback,
        string controllerFallback, string? partName = null,
        string controllerVerb = "press")
    {
        string keyboard = ReadHintKeyboardBinding(preferred, partName) ??
            ReadHintKeyboardBinding(secondary, partName) ?? keyboardFallback;
        string controller = ReadHintControllerBinding(preferred, partName,
            controllerFallback) ?? ReadHintControllerBinding(secondary,
            partName, controllerFallback) ?? controllerFallback;
        if (controllerVerb == "axis")
            controllerVerb = controller.Contains("stick",
                StringComparison.OrdinalIgnoreCase) &&
                !controller.Contains("press", StringComparison.OrdinalIgnoreCase)
                ? "move" : "press";
        string controllerInstruction = controllerVerb == "press"
            ? controller : controllerVerb + " " + controller;
        return $"Press {keyboard} on keyboard or {controllerInstruction} on controller to {purpose}.";
    }

    private static string? ReadHintKeyboardBinding(InputAction? action,
        string? partName = null)
    {
        if (action == null)
            return null;
        var labels = new List<string>(2);
        for (int index = 0; index < action.bindings.Count && labels.Count < 2; index++)
        {
            InputBinding binding = action.bindings[index];
            if (binding.isComposite || (partName != null &&
                (!binding.isPartOfComposite ||
                 !string.Equals(binding.name, partName,
                     StringComparison.OrdinalIgnoreCase))) ||
                !IsHintKeyboardPath(binding.effectivePath))
                continue;
            string? label = ReadHintBindingLabel(action, index);
            if (label != null && !labels.Contains(label))
                labels.Add(label);
        }
        return labels.Count == 0 ? null : string.Join(" or ", labels);
    }

    private static string? ReadHintControllerBinding(InputAction? action,
        string? partName, string noControllerFallback)
    {
        if (action == null)
            return null;
        var choices = new List<(string Layout, string Label)>();
        for (int index = 0; index < action.bindings.Count; index++)
        {
            InputBinding binding = action.bindings[index];
            if (binding.isComposite || (partName != null &&
                (!binding.isPartOfComposite ||
                 !string.Equals(binding.name, partName,
                     StringComparison.OrdinalIgnoreCase))))
                continue;
            string? layout = HintDeviceLayout(binding.effectivePath);
            if (!IsHintControllerLayout(layout))
                continue;
            string? label = ReadHintBindingLabel(action, index);
            if (label != null)
                choices.Add((layout!, label));
        }
        if (choices.Count == 0)
            return null;

        string? activeLayout = Gamepad.current?.layout;
        if (!string.IsNullOrEmpty(activeLayout))
        {
            // Prefer the binding for the connected controller over a generic
            // Gamepad binding. This matters for Switch face-button positions.
            foreach ((string layout, string label) in choices)
                if (!string.Equals(layout, "Gamepad",
                    StringComparison.OrdinalIgnoreCase) &&
                    HintLayoutMatches(activeLayout, layout))
                    return label;
            foreach ((string layout, string label) in choices)
                if (HintLayoutMatches(activeLayout, layout))
                    return label;
        }

        foreach ((string layout, string label) in choices)
            if (string.Equals(layout, "Gamepad",
                StringComparison.OrdinalIgnoreCase))
                return label;

        // With no matching controller connected, the same physical position
        // has different names on Xbox, PlayStation, and Switch controllers.
        return choices.Select(choice => choice.Label).Distinct().Count() == 1
            ? choices[0].Label : noControllerFallback;
    }

    private static string? HintDeviceLayout(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        try
        {
            string? layout = InputControlPath.TryGetDeviceLayout(path);
            if (!string.IsNullOrWhiteSpace(layout))
                return layout;
        }
        catch { }
        int close = path.IndexOf('>');
        return path[0] == '<' && close > 1 ? path[1..close] : null;
    }

    private static string? ReadHintBindingLabel(InputAction action, int index)
    {
        try
        {
            return CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, index));
        }
        catch
        {
            string? path = action.bindings[index].effectivePath;
            return path == null ? null : CleanSpeechValue(
                InputControlPath.ToHumanReadableString(path));
        }
    }

    private static string HintSavedBinding(string preference, string fallback)
    {
        try
        {
            string path = PlayerPrefs.GetString(preference, string.Empty);
            return string.IsNullOrWhiteSpace(path) ? fallback :
                CleanSpeechValue(InputControlPath.ToHumanReadableString(path)) ??
                fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static bool IsHintKeyboardPath(string? path) =>
        string.Equals(HintDeviceLayout(path), "Keyboard",
            StringComparison.OrdinalIgnoreCase);

    private static bool IsHintControllerLayout(string? layout)
    {
        if (layout == null)
            return false;
        if (string.Equals(layout, "Gamepad", StringComparison.OrdinalIgnoreCase))
            return true;
        try
        {
            if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad"))
                return true;
        }
        catch { }
        return layout.Contains("Gamepad", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("Controller", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("NPad", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HintLayoutMatches(string active, string binding)
    {
        if (string.Equals(active, binding, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(binding, "Gamepad", StringComparison.OrdinalIgnoreCase) ||
            active.Contains(binding, StringComparison.OrdinalIgnoreCase))
            return true;
        try
        {
            return InputSystem.IsFirstLayoutBasedOnSecond(active, binding);
        }
        catch
        {
            return false;
        }
    }

    private string HintNavigation(bool vertical, string purpose,
        InputAction? action = null)
    {
        action ??= FindHintAction("UI", "Navigate") ?? HintUiMoveAction();
        string first = vertical ? "up" : "left";
        string second = vertical ? "down" : "right";
        string keyboard = ReadHintDirections(action, first, second, false,
            vertical) ?? (vertical ? "Up and Down arrows or W and S" :
            "Left and Right arrows or A and D");
        string controller = ReadHintDirections(action, first, second, true,
            vertical) ?? (vertical ? "D-Pad or either stick up and down" :
            "D-Pad or either stick left and right");
        return $"Use {keyboard} on keyboard or {controller} on controller to {purpose}.";
    }

    private string HintAchievementPages() =>
        HintDirectionalAction(FindHintAction("UI", "FlipAchievementPages"),
            "negative", "positive", "Left and Right arrows",
            "left and right shoulder buttons", "turn pages");

    private static string HintDirectionalAction(InputAction? action,
        string firstPart, string secondPart, string keyboardFallback,
        string controllerFallback, string purpose)
    {
        string keyboard = ReadHintDirections(action, firstPart, secondPart,
            false, false) ?? keyboardFallback;
        string controller = ReadHintDirections(action, firstPart, secondPart,
            true, false) ?? controllerFallback;
        return $"Use {keyboard} on keyboard or {controller} on controller to {purpose}.";
    }

    private static string? ReadHintDirections(InputAction? action,
        string firstPart, string secondPart, bool controller, bool vertical)
    {
        if (action == null)
            return null;
        var first = new List<string>();
        var second = new List<string>();
        var direct = new List<string>();
        for (int index = 0; index < action.bindings.Count; index++)
        {
            InputBinding binding = action.bindings[index];
            if (binding.isComposite)
                continue;
            string? layout = HintDeviceLayout(binding.effectivePath);
            if (controller ? !IsHintControllerLayout(layout) :
                !string.Equals(layout, "Keyboard",
                    StringComparison.OrdinalIgnoreCase))
                continue;
            string? label = ReadHintBindingLabel(action, index);
            if (label == null)
                continue;
            if (binding.isPartOfComposite)
            {
                if (string.Equals(binding.name, firstPart,
                    StringComparison.OrdinalIgnoreCase))
                    first.Add(label);
                else if (string.Equals(binding.name, secondPart,
                    StringComparison.OrdinalIgnoreCase))
                    second.Add(label);
            }
            else if (controller && (binding.effectivePath ?? string.Empty)
                .Contains("/dpad", StringComparison.OrdinalIgnoreCase))
                direct.Add(label + (vertical ? " up and down" : " left and right"));
        }
        var pairs = new List<string>();
        for (int index = 0; index < Math.Min(first.Count, second.Count); index++)
        {
            string pair = CompactHintDirectionPair(first[index], second[index]);
            if (!pairs.Contains(pair))
                pairs.Add(pair);
        }
        foreach (string label in direct)
            if (!pairs.Contains(label))
                pairs.Add(label);
        return pairs.Count == 0 ? null : string.Join(" or ", pairs);
    }

    private static string CompactHintDirectionPair(string first, string second)
    {
        if ((first.Equals("Up", StringComparison.OrdinalIgnoreCase) ||
             first.Equals("Up Arrow", StringComparison.OrdinalIgnoreCase)) &&
            (second.Equals("Down", StringComparison.OrdinalIgnoreCase) ||
             second.Equals("Down Arrow", StringComparison.OrdinalIgnoreCase)))
            return "Up and Down arrows";
        if ((first.Equals("Left", StringComparison.OrdinalIgnoreCase) ||
             first.Equals("Left Arrow", StringComparison.OrdinalIgnoreCase)) &&
            (second.Equals("Right", StringComparison.OrdinalIgnoreCase) ||
             second.Equals("Right Arrow", StringComparison.OrdinalIgnoreCase)))
            return "Left and Right arrows";
        foreach (string stick in new[] { "Left Stick", "Right Stick" })
        {
            if (first.Equals(stick + " Up", StringComparison.OrdinalIgnoreCase) &&
                second.Equals(stick + " Down", StringComparison.OrdinalIgnoreCase))
                return stick + " up and down";
            if (first.Equals(stick + " Left", StringComparison.OrdinalIgnoreCase) &&
                second.Equals(stick + " Right", StringComparison.OrdinalIgnoreCase))
                return stick + " left and right";
        }
        return first + " and " + second;
    }

    private string HintLeaderboardRows(bool controllerMoveAvailable)
    {
        const string keyboard = "Use Page Up and Page Down on keyboard to read score rows.";
        if (!controllerMoveAvailable)
            return keyboard;
        InputAction? move = HintUiMoveAction();
        string? controller = ReadHintDirections(move, "up", "down", true, true);
        return controller == null ? keyboard : keyboard +
            $" Use {controller} on controller when no menu control has focus.";
    }
}
