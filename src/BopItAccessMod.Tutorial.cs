using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // Tutorial panels are only displayed during timed play. Read the native
    // text from the panel selected for the current mode or difficulty when
    // the player explicitly requests hints on the song-selection screen.
    private string? ReadPreRoundTutorialSummary()
    {
        GameUIManager? ui = _trackSelectUi;
        GameManager? game = ui?.gameManager;
        if (ui == null || game == null ||
            game.GameState != GameState.WaitingToStart ||
            !IsHintPanelVisible(ui.startScreen))
            return null;

        Panel? panel;
        if (game.GameMode == GameMode.OneOnOne)
            panel = ui.oneOnOneTutorialPanel;
        else
        {
            bool? extreme = ReadTrackSelectExtreme(ui);
            panel = extreme.HasValue
                ? extreme.Value ? ui.extremeTutorialPanel : ui.classicTutorialPanel
                : null;
        }

        if (panel == null)
            return null;

        Player? player = game.Player;
        if (player == null)
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        var nativeLines = new List<(string Text, string? Action)>();
        foreach (TMP_Text label in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (label == null || !label.enabled)
                continue;
            // The panel itself is hidden until play starts. Its inactive
            // descendants can be alternate layouts or placeholder text.
            // Read only labels whose own branch will be shown with the panel.
            bool visibleWithPanel = true;
            for (var node = label.transform; node != null &&
                 node != panel.transform; node = node.parent)
            {
                if (!node.gameObject.activeSelf)
                {
                    visibleWithPanel = false;
                    break;
                }
            }
            if (!visibleWithPanel)
                continue;
            string? line = CleanSpeechValue(label.text);
            if (line == null)
                continue;
            string normalized = line.TrimEnd('.', '!', '?').Trim();
            if (normalized.Length == 0)
                continue;
            // The prompt GameObject names are stable across the game's
            // translations; the visible text is not. Keep the native words
            // but identify their Gameplay actions from those prompt names.
            string? action = TutorialGameplayAction(label.transform,
                panel.transform) ?? TutorialGameplayAction(normalized);
            nativeLines.Add((normalized, action));
        }

        // A prompt can contain more than one text label. Put its binding on
        // the actual action label when we can recognize it in English; in
        // other languages, use the first visible label for that prompt.
        var bindingLine = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < nativeLines.Count; index++)
        {
            var item = nativeLines[index];
            if (item.Action == null)
                continue;
            if (!bindingLine.TryGetValue(item.Action, out int prior) ||
                (TutorialGameplayAction(item.Text) == item.Action &&
                 TutorialGameplayAction(nativeLines[prior].Text) != item.Action))
                bindingLine[item.Action] = index;
        }

        bool hasSecondBop = bindingLine.ContainsKey("AltBop");
        var lines = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int characters = 0;
        for (int index = 0; index < nativeLines.Count; index++)
        {
            var item = nativeLines[index];
            // Keep distinct Bop and AltBop prompts even when their visible
            // labels use the same words.
            if (!seen.Add((item.Action ?? "") + "\u001f" + item.Text))
                continue;
            string spoken = item.Action != null &&
                bindingLine[item.Action] == index
                ? AddTutorialActionBinding(item.Text, player, item.Action,
                    game.GameMode == GameMode.OneOnOne, hasSecondBop)
                : item.Text;
            lines.Add(spoken);
            characters += spoken.Length;
            if (lines.Count >= 16 || characters >= 1200)
                break;
        }

        return lines.Count == 0 ? null :
            L("Tutorial reference.") + " " + string.Join(". ", lines) + ".";
    }

    private string AddTutorialActionBinding(string line, Player? player,
        string actionName, bool oneOnOne, bool hasSecondBop)
    {
        string binding = ReadTutorialGameplayBinding(player, actionName);
        binding = LocalizeBindingDisplay(binding);
        if (oneOnOne && actionName == "AltBop")
            return LF("{0}, Player 2: {1}", line, binding);
        if (oneOnOne && actionName == "Bop")
        {
            if (hasSecondBop)
                return LF("{0}, Player 1: {1}", line, binding);
            // Some layouts can hide the second prompt. Still identify both
            // Bop controls when its own visible label is unavailable.
            string second = ReadTutorialGameplayBinding(player, "AltBop");
            second = LocalizeBindingDisplay(second);
            return LF("{0}, Player 1: {1}; Player 2: {2}", line,
                binding, second);
        }

        return LF("{0}, {1}", line, binding);
    }

    private static string? TutorialGameplayAction(Transform label,
        Transform panel)
    {
        for (Transform? node = label; node != null && node != panel;
             node = node.parent)
        {
            switch (node.gameObject.name)
            {
                case "AltBopItTutorialPrompt": return "AltBop";
                case "BopItTutorialPrompt": return "Bop";
                case "PullItTutorialPrompt": return "Pull";
                case "TwistTutorialPrompt": return "Twist";
                case "FlickItPrompt": return "Flick";
                case "SpinItTutorialPrompt": return "Spin";
            }
        }
        return null;
    }

    private static string? TutorialGameplayAction(string line)
    {
        foreach (string action in new[] { "Pull", "Bop", "Twist", "Flick", "Spin" })
        {
            string phrase = action + " it";
            int position = line.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);
            int end = position + phrase.Length;
            if (line.Equals(action, StringComparison.OrdinalIgnoreCase) ||
                (position >= 0 &&
                 (position == 0 || !char.IsLetterOrDigit(line[position - 1])) &&
                 (end == line.Length || !char.IsLetterOrDigit(line[end]))))
                return action;
        }
        return null;
    }

    private string ReadTutorialGameplayBinding(Player? player,
        string actionName)
    {
        // Use the actual player in this game scene. The main-menu Controls
        // asset can disappear or lag behind its saved runtime overrides.
        InputAction? action = player?.playerInput?.actions?
            .FindActionMap("Gameplay", false)?.FindAction(actionName, false) ??
            player?.gameplayActionMap?.FindAction(actionName, false) ??
            FindHintAction("Gameplay", actionName);
        if (action == null)
            return L("binding unavailable");

        string? rawKeyboard = ReadHintKeyboardBinding(action);
        string? keyboard = rawKeyboard == null ? null :
            LocalizeBindingDisplay(rawKeyboard);
        string? controller = ReadTutorialControllerBinding(action);
        return EffectiveHintDevice switch
        {
            HintDevice.Keyboard => keyboard ?? L("unassigned on keyboard"),
            HintDevice.Controller => controller ?? L("unassigned on controller"),
            _ => LF("{0} and {1}",
                TutorialDeviceBinding(keyboard, "keyboard"),
                TutorialDeviceBinding(controller, "controller"))
        };
    }

    private static string TutorialDeviceBinding(string? binding, string device)
    {
        if (binding == null)
            return LF("unassigned on {0}", L(device));
        // A layout-specific fallback already identifies its controller.
        if (device == "controller" &&
            (binding.Contains(" controller", StringComparison.OrdinalIgnoreCase) ||
             binding.Contains(L("controller"), StringComparison.OrdinalIgnoreCase)))
            return binding;
        return LF("{0} on {1}", binding, L(device));
    }

    private static string? ReadTutorialControllerBinding(InputAction action)
    {
        // The ordinary hint helper chooses the connected controller's
        // effective binding. If no controller is connected and the game has
        // different platform layouts, list their real assigned alternatives
        // instead of falling back to a hard-coded default button.
        var alternatives = new List<string>();
        for (int index = 0; index < action.bindings.Count; index++)
        {
            InputBinding binding = action.bindings[index];
            if (binding.isComposite)
                continue;
            string? layout = HintDeviceLayout(binding.effectivePath);
            if (!IsHintControllerLayout(layout))
                continue;
            string? label = ReadHintBindingLabel(action, index);
            if (label == null)
                continue;
            string alternative = LF("{0} on {1}",
                LocalizeBindingDisplay(label),
                TutorialControllerLayout(layout!));
            if (!alternatives.Contains(alternative, StringComparer.OrdinalIgnoreCase))
                alternatives.Add(alternative);
        }
        if (alternatives.Count == 0)
            return null;
        string fallback = alternatives.Count == 1 ? alternatives[0] :
            LF("{0} or {1}", string.Join(L(", "), alternatives.Take(
                alternatives.Count - 1)), alternatives[^1]);
        string? selected = ReadHintControllerBinding(action, null, fallback);
        return selected == null || selected == fallback ? selected :
            LocalizeBindingDisplay(selected);
    }

    private static string TutorialControllerLayout(string layout)
    {
        if (layout.Contains("DualSense", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("DualShock", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("PlayStation", StringComparison.OrdinalIgnoreCase))
            return L("PlayStation controller");
        if (layout.Contains("Switch", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("NPad", StringComparison.OrdinalIgnoreCase))
            return L("Switch controller");
        if (layout.Contains("Xbox", StringComparison.OrdinalIgnoreCase) ||
            layout.Contains("XInput", StringComparison.OrdinalIgnoreCase))
            return L("Xbox controller");
        return layout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase)
            ? L("controller") : LF("{0} controller", layout);
    }
}
