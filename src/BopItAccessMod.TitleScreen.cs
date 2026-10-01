using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private TitleScreen? _titleScreen;
    private App? _titleApp;
    private MainMenuUIManager? _titleMainMenu;
    private long _nextTitleScreenSearchAt;
    private long _nextTitleScreenErrorLogAt;
    private int _lastTitleScreenId;

    // The title scene listens for Bop and opens the main menu. It has no
    // selectable menu item for the ordinary focus reader to announce.
    private bool ReadTitleScreen()
    {
        long now = Environment.TickCount64;
        if (_titleScreen == null)
        {
            _lastTitleScreenId = 0;
            if (now < _nextTitleScreenSearchAt)
                return false;

            _nextTitleScreenSearchAt = now + 500;
            _titleScreen = UnityEngine.Object.FindFirstObjectByType<TitleScreen>();
            if (_titleScreen == null)
                return false;
        }

        EventSystem? eventSystem = EventSystem.current;
        if (!_titleScreen.gameObject.activeInHierarchy || !_titleScreen.enabled ||
            eventSystem == null || !eventSystem.enabled)
        {
            _lastTitleScreenId = 0;
            return false;
        }

        // The TitleScreen component can outlive its visible title overlay.
        // A selected control or an open main-menu panel is stronger evidence
        // that the player has moved on than the component's active flag.
        if (eventSystem.currentSelectedGameObject != null)
        {
            _lastTitleScreenId = 0;
            return false;
        }
        if (_titleMainMenu == null)
            _titleMainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        if (IsHintPanelVisible(_titleMainMenu?.mainMenuPanel) ||
            IsHintPanelVisible(_titleMainMenu?.gameModePanel) ||
            IsHintPanelVisible(_titleMainMenu?.settingsPanel) ||
            IsHintPanelVisible(_titleMainMenu?.leaderboardPanel) ||
            IsHintPanelVisible(_titleMainMenu?.creditsPanel))
        {
            _lastTitleScreenId = 0;
            return false;
        }

        if (_titleApp == null)
            _titleApp = UnityEngine.Object.FindFirstObjectByType<App>();
        if (_titleApp != null && _titleApp.PassedTitleScreen)
        {
            _lastTitleScreenId = 0;
            return false;
        }

        int id = _titleScreen.GetInstanceID();
        if (id != _lastTitleScreenId)
        {
            _lastTitleScreenId = id;
            string bop = HintNativeAction("Bop", "open main menu",
                "Space", "confirm button");
            string instruction = _readButtonHintsEnabled &&
                _buttonHintsDelaySeconds == 0
                ? WithGlobalControlHints(bop) : bop;
            // The mod's startup notice may still be speaking. Queue this
            // instruction behind it, while a later menu focus may interrupt.
            QueueSpeech("Title screen. " + instruction, interrupt: false);
            WriteStatus("Title screen visible; announced Bop to open main menu.");
        }

        return true;
    }
}
