using Il2Cpp;
using Il2CppTMPro;

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

        var lines = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int characters = 0;
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
            if (normalized.Length == 0 || !seen.Add(normalized))
                continue;
            lines.Add(normalized);
            characters += normalized.Length;
            if (lines.Count >= 16 || characters >= 1200)
                break;
        }

        return lines.Count == 0 ? null :
            "Tutorial reference. " + string.Join(". ", lines) + ".";
    }
}
