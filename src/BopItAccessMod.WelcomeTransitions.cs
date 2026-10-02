using Il2Cpp;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private bool OpenUserGuideFromWelcome() => OpenGuide();

    private bool OpenModSettingsFromWelcome()
    {
        MainMenuUIManager? main = _mainMenu;
        if (main?.panels == null || main.panels.Count == 0 ||
            main.mainMenuPanel == null ||
            main.panels.Peek().GetInstanceID() !=
                main.mainMenuPanel.GetInstanceID() ||
            main.settingsButton == null)
            return false;

        bool enteredSettings = false;
        try
        {
            // Use the game's own Settings button route so its panel stack and
            // Back behavior stay identical to ordinary menu navigation.
            main.settingsButton.onClick.Invoke();
            SettingsPanel? settings = main.settingsPanel?
                .GetComponent<SettingsPanel>() ??
                UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
            if (settings == null || !settings.IsVisible ||
                !settings.gameObject.activeInHierarchy)
                return false;
            enteredSettings = true;
            _settingsPanel = settings;
            _mainMenu = main;
            _nextSpeechSettingsProbeAt = 0;
            UpdateSpeechMenuUi();
            OpenSpeechMenu();
            return _speechMenuOpen && _speechMenuPanel != null &&
                _speechMenuPanel.IsVisible && main.panels.Count > 0 &&
                main.panels.Peek().GetInstanceID() ==
                    _speechMenuPanel.GetInstanceID();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not open Mod Settings from welcome: " + ex);
            return false;
        }
        finally
        {
            if (enteredSettings && !_speechMenuOpen &&
                main.panels != null && main.panels.Count > 0 &&
                main.settingsPanel != null &&
                main.panels.Peek().GetInstanceID() ==
                    main.settingsPanel.GetInstanceID())
            {
                try { main.GoBack(); }
                catch (Exception ex)
                {
                    WriteStatus("Could not restore main menu after welcome " +
                        "Settings failure: " + ex.Message);
                }
            }
        }
    }
}
