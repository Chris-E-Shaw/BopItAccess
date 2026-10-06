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
        if (string.Equals(mapName, "Gameplay", StringComparison.Ordinal))
        {
            // Mode and song selection run in a separate scene from Controls.
            // Read the active player's action copy first: it contains the
            // saved overrides currently used by the game, even when the
            // main-menu rebinding manager has been destroyed.
            GameUIManager? gameUi = _trackSelectUi;
            Player? player = gameUi == null ? null : gameUi.gameManager?.Player;
            InputAction? liveAction = player?.playerInput?.actions?
                .FindActionMap(mapName, false)?.FindAction(actionName, false) ??
                player?.gameplayActionMap?.FindAction(actionName, false);
            if (liveAction != null)
                return liveAction;
        }

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
        string keyboardFallback, string controllerFallback) =>
        HintActionSentence(FindHintAction("Gameplay", name), null, purpose,
            keyboardFallback, controllerFallback);

    private string HintActionSentence(InputAction? preferred,
        InputAction? secondary, string purpose, string keyboardFallback,
        string controllerFallback, string? partName = null)
    {
        string keyboard = ReadHintKeyboardBinding(preferred, partName) ??
            ReadHintKeyboardBinding(secondary, partName) ?? keyboardFallback;
        string controller = ReadHintControllerBinding(preferred, partName,
            controllerFallback) ?? ReadHintControllerBinding(secondary,
            partName, controllerFallback) ?? controllerFallback;
        return FormatHintPress(keyboard, controller, purpose);
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
        return labels.Count == 0 ? null : JoinHintChoices(labels);
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
            return ExpandHintStickLabel(CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, index)));
        }
        catch
        {
            string? path = action.bindings[index].effectivePath;
            return path == null ? null : ExpandHintStickLabel(CleanSpeechValue(
                InputControlPath.ToHumanReadableString(path)));
        }
    }

    private static string? ExpandHintStickLabel(string? label)
    {
        if (label == null)
            return null;
        if (label.Equals("LS", StringComparison.OrdinalIgnoreCase))
            return "Left stick";
        if (label.StartsWith("LS ", StringComparison.OrdinalIgnoreCase))
            return "Left stick " + label[3..];
        if (label.Equals("RS", StringComparison.OrdinalIgnoreCase))
            return "Right stick";
        if (label.StartsWith("RS ", StringComparison.OrdinalIgnoreCase))
            return "Right stick " + label[3..];
        if (label.StartsWith("Left Stick", StringComparison.OrdinalIgnoreCase))
            return "Left stick" + label["Left Stick".Length..];
        if (label.StartsWith("Right Stick", StringComparison.OrdinalIgnoreCase))
            return "Right stick" + label["Right Stick".Length..];
        return label;
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
            vertical) ?? (vertical
                ? "Left stick Up and Down, Right stick Up and Down, or D-Pad up and down"
                : "Left stick Left and Right, Right stick Left and Right, or D-Pad left and right");
        return FormatHintUse(keyboard, controller, purpose);
    }

    private string HintAchievementPages() =>
        HintDirectionalAction(FindHintAction("UI", "FlipAchievementPages"),
            "negative", "positive", "Left and Right arrows",
            "Left and Right shoulder buttons", "turn pages");

    private string HintDirectionalAction(InputAction? action,
        string firstPart, string secondPart, string keyboardFallback,
        string controllerFallback, string purpose)
    {
        string keyboard = ReadHintDirections(action, firstPart, secondPart,
            false, false) ?? keyboardFallback;
        string controller = ReadHintDirections(action, firstPart, secondPart,
            true, false) ?? controllerFallback;
        return FormatHintUse(keyboard, controller, purpose);
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
                direct.Add(vertical
                    ? label + " up and down"
                    : label + " left and right");
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
        return pairs.Count == 0 ? null : JoinHintChoices(pairs);
    }

    private static string JoinHintChoices(IReadOnlyList<string> choices)
    {
        if (choices.Count == 1)
            return choices[0];
        if (choices.Count == 2)
            return choices[0] + " or " + choices[1];
        return string.Join(", ", choices.Take(choices.Count - 1)) +
            ", or " + choices[^1];
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
        if (first.Equals("D-Pad Up", StringComparison.OrdinalIgnoreCase) &&
            second.Equals("D-Pad Down", StringComparison.OrdinalIgnoreCase))
            return "D-Pad up and down";
        if (first.Equals("D-Pad Left", StringComparison.OrdinalIgnoreCase) &&
            second.Equals("D-Pad Right", StringComparison.OrdinalIgnoreCase))
            return "D-Pad left and right";
        foreach ((string binding, string spoken) in new[]
        {
            ("Left Stick", "Left stick"), ("LS", "Left stick"),
            ("Right Stick", "Right stick"), ("RS", "Right stick")
        })
        {
            if (first.Equals(binding + " Up", StringComparison.OrdinalIgnoreCase) &&
                second.Equals(binding + " Down", StringComparison.OrdinalIgnoreCase))
                return spoken + " Up and Down";
            if (first.Equals(binding + " Left", StringComparison.OrdinalIgnoreCase) &&
                second.Equals(binding + " Right", StringComparison.OrdinalIgnoreCase))
                return spoken + " Left and Right";
        }
        return first + " and " + second;
    }

    private string HintLeaderboardRows(bool controllerMoveAvailable)
    {
        string keyboard = L("Page Up and Page Down, read score rows.");
        string bothKeyboard = L("Page Up and Page Down on keyboard, read score rows.");
        if (!controllerMoveAvailable)
            return EffectiveHintDevice switch
            {
                HintDevice.Controller => string.Empty,
                HintDevice.Both => bothKeyboard,
                _ => keyboard
            };
        InputAction? move = HintUiMoveAction();
        string? controller = ReadHintDirections(move, "up", "down", true, true);
        string controllerHint = controller == null ? string.Empty :
            LF("{0}, read score rows.", LocalizeBindingDisplay(controller));
        string bothController = controller == null ? string.Empty :
            LF("{0} on controller, read score rows.",
                LocalizeBindingDisplay(controller));
        return EffectiveHintDevice switch
        {
            HintDevice.Keyboard => keyboard,
            HintDevice.Controller => controllerHint,
            _ => bothController.Length == 0 ? bothKeyboard :
                bothKeyboard + " " + bothController
        };
    }
}
