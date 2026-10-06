using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string WelcomeDismissedPreferenceKey =
        "BopItAccess.WelcomeDismissed";

    private static BopItAccessMod? _activeWelcomeMod;
    private GameObject? _welcomeRoot;
    private Panel? _welcomePanel;
    private ScrollRect? _welcomeScroll;
    private TMP_Text? _welcomeTitle;
    private string? _welcomeRenderedLocale;
    private SettingsButton? _welcomeIntroRow;
    private SettingsButton? _welcomeSettingsRow;
    private SettingsButton? _welcomeGuideRow;
    private SettingsButton? _welcomeContinueRow;
    private string _welcomeIntroText = string.Empty;
    private bool _welcomeOpen;
    private bool _welcomeInputReady;
    private bool _welcomeAllowBack;
    private bool _welcomeSuppressedUntilRestart;
    private int _welcomeOpenedFrame;
    private int _welcomeFocusedRowId;
    private int _welcomeMainMenuId;
    private long _nextWelcomeProbeAt;
    private long _nextWelcomeErrorAt;
    private UnityAction? _welcomeIntroSubmitListener;
    private UnityAction? _welcomeSettingsSubmitListener;
    private UnityAction? _welcomeGuideSubmitListener;
    private UnityAction? _welcomeContinueSubmitListener;

    private bool WelcomeScreenOpen => _welcomeOpen &&
        _welcomeRoot != null && _welcomeRoot.activeInHierarchy &&
        IsHintPanelVisible(_welcomePanel);

    // Called before the ordinary menu readers. The game's main-menu panel is
    // the signal that startup and the title screen are fully behind us.
    private void UpdateWelcomeScreen()
    {
        _activeWelcomeMod = this;
        if (_welcomeOpen)
        {
            RefreshWelcomeLocalizedLabels();
            if (_welcomeRoot == null || _welcomePanel == null ||
                _mainMenu == null || _mainMenu.panels == null ||
                _mainMenu.panels.Count == 0 ||
                _mainMenu.panels.Peek().GetInstanceID() !=
                    _welcomePanel.GetInstanceID())
            {
                _welcomeOpen = false;
                _welcomeInputReady = false;
                _welcomeFocusedRowId = 0;
                return;
            }

            if (!_welcomeInputReady && Time.frameCount > _welcomeOpenedFrame &&
                !IsUiSubmitHeld())
            {
                _welcomeInputReady = true;
                GameObject? selected = EventSystem.current?.currentSelectedGameObject;
                if (_welcomeIntroRow != null &&
                    (selected == null ||
                     !selected.transform.IsChildOf(_welcomeRoot.transform)))
                    EventSystem.current?.SetSelectedGameObject(
                        _welcomeIntroRow.gameObject);
            }
            ScrollSelectedRowIntoView(_welcomeScroll);
            return;
        }

        if (_welcomeSuppressedUntilRestart ||
            PlayerPrefs.GetInt(WelcomeDismissedPreferenceKey, 0) != 0)
            return;

        long now = Environment.TickCount64;
        if (now < _nextWelcomeProbeAt)
            return;
        _nextWelcomeProbeAt = now + 250;

        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        if (main == null || main.panels == null || main.panels.Count == 0 ||
            main.mainMenuPanel == null || !main.mainMenuPanel.IsVisible ||
            !main.mainMenuPanel.gameObject.activeInHierarchy ||
            main.panels.Peek().GetInstanceID() !=
                main.mainMenuPanel.GetInstanceID())
            return;

        try
        {
            _mainMenu = main;
            if (_welcomeRoot == null || _welcomePanel == null ||
                _welcomeMainMenuId != main.GetInstanceID())
                BuildWelcomeScreen(main);
            OpenWelcomeScreen(main);
        }
        catch (Exception ex)
        {
            if (now >= _nextWelcomeErrorAt)
            {
                _nextWelcomeErrorAt = now + 5000;
                WriteStatus("Welcome screen could not open; will retry: " + ex);
                MelonLoader.MelonLogger.Warning(
                    "Welcome screen could not open; will retry: " + ex.Message);
            }
        }
    }

    private void BuildWelcomeScreen(MainMenuUIManager main)
    {
        Panel? controls = main.controlsPanel;
        SettingsButton? buttonTemplate =
            main.settingsPanel?.GetComponent<SettingsPanel>()?.controls;
        RectTransform? sourceRect = controls?.GetComponent<RectTransform>();
        Transform? sourceLayout = controls?.transform.Find("Layout");
        if (controls == null || buttonTemplate == null || sourceRect == null ||
            sourceLayout == null || controls.transform.parent == null)
            throw new InvalidOperationException(
                "The native Controls panel or Settings button is unavailable.");

        if (_welcomeRoot != null)
            UnityEngine.Object.Destroy(_welcomeRoot);
        _welcomeRoot = null;
        _welcomePanel = null;
        _welcomeIntroRow = null;
        _welcomeSettingsRow = null;
        _welcomeGuideRow = null;
        _welcomeContinueRow = null;
        _welcomeScroll = null;

        GameObject root = new("BopItAccess Welcome Screen",
            new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
        root.SetActive(false);
        try
        {
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(controls.transform.parent, false);
            rect.anchorMin = sourceRect.anchorMin;
            rect.anchorMax = sourceRect.anchorMax;
            rect.pivot = sourceRect.pivot;
            rect.anchoredPosition = sourceRect.anchoredPosition;
            rect.sizeDelta = sourceRect.sizeDelta;
            rect.localScale = sourceRect.localScale;
            rect.localRotation = sourceRect.localRotation;

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            Panel panel = root.AddComponent<Panel>();
            panel.canvasGroup = group;

            GameObject layout = UnityEngine.Object.Instantiate(
                sourceLayout.gameObject, root.transform, false);
            Transform? table = layout.transform.Find("Table");
            Transform? content = table?.Find("Content") ??
                table?.Find("BopItAccess Controls Viewport/Content");
            TMP_Text? title = table?.Find("Title")?.GetComponent<TMP_Text>();
            if (table == null || content == null || title == null)
                throw new InvalidOperationException(
                    "The cloned Controls table is incomplete.");
            SetClonedLabel(title, L("WELCOME"));
            _welcomeTitle = title;
            _welcomeRenderedLocale = CurrentGameLocale;
            for (int i = 0; i < content.childCount; i++)
                content.GetChild(i).gameObject.SetActive(false);

            _welcomeIntroText = ComposeWelcomeIntroText();
            _welcomeIntroRow = AddSpeechButton(buttonTemplate, content,
                _welcomeIntroText,
                _welcomeIntroSubmitListener ??= (UnityAction)OnWelcomeIntroSubmitted);
            _welcomeIntroRow.gameObject.name = "BopItAccess Welcome Text";
            FormatWelcomeIntroRow(_welcomeIntroRow);
            _welcomeSettingsRow = AddSpeechButton(buttonTemplate, content,
                "Open mod settings, recommended",
                _welcomeSettingsSubmitListener ??=
                    (UnityAction)OnWelcomeSettingsSubmitted);
            _welcomeGuideRow = AddSpeechButton(buttonTemplate, content,
                "Read user's guide",
                _welcomeGuideSubmitListener ??=
                    (UnityAction)OnWelcomeGuideSubmitted);
            _welcomeContinueRow = AddSpeechButton(buttonTemplate, content,
                "Continue to game",
                _welcomeContinueSubmitListener ??=
                    (UnityAction)OnWelcomeContinueSubmitted);

            panel.firstSelectedButton = _welcomeIntroRow.gameObject;
            panel.lastSelectedButton = null;
            RectTransform? tableRect = table.GetComponent<RectTransform>();
            RectTransform? contentRect = content.GetComponent<RectTransform>();
            if (tableRect != null && contentRect != null)
            {
                _welcomeScroll = tableRect.GetComponent<ScrollRect>();
                if (_welcomeScroll == null)
                    _welcomeScroll = AddSpeechViewport(tableRect, contentRect,
                        "BopItAccess Welcome Viewport");
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                if (_welcomeScroll != null)
                    _welcomeScroll.verticalNormalizedPosition = 1f;
            }

            _welcomeRoot = root;
            _welcomePanel = panel;
            _welcomeMainMenuId = main.GetInstanceID();
            WriteStatus("Built welcome screen with focusable message and three choices.");
        }
        catch
        {
            UnityEngine.Object.Destroy(root);
            _welcomeRoot = null;
            _welcomePanel = null;
            _welcomeIntroRow = null;
            _welcomeSettingsRow = null;
            _welcomeGuideRow = null;
            _welcomeContinueRow = null;
            _welcomeScroll = null;
            _welcomeTitle = null;
            throw;
        }
    }

    private void RefreshWelcomeLocalizedLabels()
    {
        string locale = CurrentGameLocale;
        if (string.Equals(_welcomeRenderedLocale, locale,
            StringComparison.Ordinal))
            return;
        _welcomeRenderedLocale = locale;
        if (_welcomeTitle != null)
            SetClonedLabel(_welcomeTitle, L("WELCOME"));
        _welcomeIntroText = ComposeWelcomeIntroText();
        if (_welcomeIntroRow != null)
            SetSpeechRowLabel(_welcomeIntroRow, _welcomeIntroText);
        if (_welcomeSettingsRow != null)
            SetSpeechRowLabel(_welcomeSettingsRow,
                "Open mod settings, recommended");
        if (_welcomeGuideRow != null)
            SetSpeechRowLabel(_welcomeGuideRow, "Read user's guide");
        if (_welcomeContinueRow != null)
            SetSpeechRowLabel(_welcomeContinueRow, "Continue to game");
        _welcomeFocusedRowId = 0;
    }

    private string ComposeWelcomeIntroText()
    {
        string keyboard = "H";
        string controller = "right stick press";
        try
        {
            InputAction action = EnsureSpeakHintsAction();
            keyboard = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ??
                keyboard;
            controller = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ??
                controller;
        }
        catch (Exception ex)
        {
            WriteStatus("Welcome screen could not read Speak Hints bindings: " +
                ex.Message);
            keyboard = HintSavedBinding(SpeakHintsKeyboardKey, keyboard);
            controller = HintSavedBinding(SpeakHintsGamepadKey, controller);
        }

        return L("Welcome to Bop It Access! Thank you for installing this project.") +
            " " + LF("Hints for every screen, including this one, are available at any time: {0} on keyboard or {1} on controller.",
                LocalizeBindingDisplay(keyboard),
                LocalizeBindingDisplay(controller)) + " " +
            L("We recommend visiting Mod Settings to customize the experience before playing.") +
            " " + L("If you need more help, a full user's guide is available.") +
            " " + L("What would you like to do?");
    }

    private static void FormatWelcomeIntroRow(SettingsButton row)
    {
        LayoutElement? layout = row.GetComponent<LayoutElement>();
        if (layout == null)
            layout = row.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 480f;
        layout.preferredHeight = 480f;
        RectTransform? rect = row.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 480f);

        TMP_Text? regular = FindFpsLabel(row.DefaultContainer, row.ValueText);
        TMP_Text? active = FindFpsLabel(row.ActiveContainer, row.ActiveValueText);
        foreach (TMP_Text? text in new[] { regular, active })
        {
            if (text == null)
                continue;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.fontSize = 20f;
            RectTransform labelRect = text.rectTransform;
            labelRect.sizeDelta = new Vector2(labelRect.sizeDelta.x, 440f);
        }
    }

    private void OpenWelcomeScreen(MainMenuUIManager main)
    {
        if (_welcomePanel == null || _welcomeRoot == null ||
            _welcomeIntroRow == null)
            throw new InvalidOperationException("The welcome panel is unavailable.");

        bool hidden = false;
        bool pushed = false;
        try
        {
            main.mainMenuPanel.Hide();
            hidden = true;
            _welcomeRoot.SetActive(true);
            _welcomePanel.lastSelectedButton = null;
            _welcomePanel.firstSelectedButton = _welcomeIntroRow.gameObject;
            _welcomeOpenedFrame = Time.frameCount;
            _welcomeInputReady = false;
            _welcomePanel.Show();
            main.panels.Push(_welcomePanel);
            pushed = true;
            EventSystem.current?.SetSelectedGameObject(null);
            _welcomeFocusedRowId = 0;
            _welcomeOpen = true;
            WriteStatus("Opened first-run welcome screen over main menu.");
        }
        catch
        {
            if (pushed && main.panels.Count > 0 &&
                main.panels.Peek().GetInstanceID() ==
                    _welcomePanel.GetInstanceID())
                main.panels.Pop();
            _welcomeRoot.SetActive(false);
            if (hidden)
                main.mainMenuPanel.Show();
            _welcomeOpen = false;
            throw;
        }
    }

    private bool ReadWelcomeScreenFocus()
    {
        if (!WelcomeScreenOpen)
        {
            _welcomeFocusedRowId = 0;
            return false;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        SettingsRow? row = selected?.GetComponentInParent<SettingsRow>();
        SettingsButton?[] rows =
        {
            _welcomeIntroRow, _welcomeSettingsRow, _welcomeGuideRow,
            _welcomeContinueRow
        };
        int index = -1;
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null)
                continue;
            if ((row != null && row.GetInstanceID() ==
                     rows[i]!.GetInstanceID()) || rows[i]!.IsFocused)
            {
                index = i;
                break;
            }
        }
        if (index < 0)
            return true;

        int id = rows[index]!.GetInstanceID();
        if (id == _welcomeFocusedRowId)
            return true;
        _welcomeFocusedRowId = id;
        string announcement = index switch
        {
            0 => L("Welcome message.") + " " + _welcomeIntroText,
            1 => WithControlType(L("Open mod settings, recommended"), "button"),
            2 => WithControlType(L("Read user's guide"), "button"),
            _ => WithControlType(L("Continue to game"), "button")
        };
        QueueFocusSpeech(WithMenuIndex(announcement, index, rows.Length));
        return true;
    }

    private string WelcomeScreenHint() => WithGlobalControlHints(
        HintNavigation(true, "choose a welcome item") + " " +
        UiSubmitHint("open the selected option"));

    private void OnWelcomeIntroSubmitted()
    {
        // The message is selectable so players can revisit and hear it.
    }

    private void OnWelcomeSettingsSubmitted()
    {
        if (!_welcomeOpen || !_welcomeInputReady)
            return;
        TryLeaveWelcome(OpenModSettingsFromWelcome);
    }

    private void OnWelcomeGuideSubmitted()
    {
        if (!_welcomeOpen || !_welcomeInputReady)
            return;
        TryLeaveWelcome(OpenUserGuideFromWelcome);
    }

    private void OnWelcomeContinueSubmitted()
    {
        if (!_welcomeOpen || !_welcomeInputReady)
            return;
        TryLeaveWelcome(() => true);
    }

    private void TryLeaveWelcome(Func<bool> openDestination)
    {
        bool moved = false;
        try
        {
            if (!CloseWelcomeForTransition())
                return;
            moved = openDestination();
            if (moved)
                CommitWelcomeDismissal();
        }
        catch (Exception ex)
        {
            WriteStatus("Welcome destination could not open: " + ex);
            moved = false;
        }
        if (!moved)
            RestoreWelcomeAfterFailedTransition();
    }

    // Pop through the same native route used by the game's Back action.
    // This does not persist anything; callers commit only when their target
    // screen has actually opened.
    private bool CloseWelcomeForTransition()
    {
        MainMenuUIManager? main = _mainMenu;
        if (!_welcomeOpen || main == null || _welcomePanel == null ||
            main.panels == null || main.panels.Count == 0 ||
            main.panels.Peek().GetInstanceID() != _welcomePanel.GetInstanceID())
            return false;

        try
        {
            _welcomeAllowBack = true;
            main.GoBack();
        }
        finally
        {
            _welcomeAllowBack = false;
        }
        if (!main.mainMenuPanel.IsVisible ||
            !main.mainMenuPanel.gameObject.activeInHierarchy)
            return false;
        _welcomeOpen = false;
        _welcomeInputReady = false;
        _welcomeFocusedRowId = 0;
        return true;
    }

    private void RestoreWelcomeAfterFailedTransition()
    {
        MainMenuUIManager? main = _mainMenu;
        if (main == null || _welcomeOpen ||
            !main.mainMenuPanel.IsVisible)
            return;
        try
        {
            OpenWelcomeScreen(main);
            QueueSpeech(L("That screen could not be opened. Please choose another option."));
        }
        catch (Exception ex)
        {
            WriteStatus("Could not restore the welcome screen: " + ex);
        }
    }

    private void CommitWelcomeDismissal()
    {
        PlayerPrefs.SetInt(WelcomeDismissedPreferenceKey, 1);
        SaveModPreferencesAndConfig();
        _welcomeSuppressedUntilRestart = true;
        WriteStatus("Welcome screen dismissed after the selected destination opened.");
    }

    private bool ResetWelcomeScreenForNextLaunch()
    {
        try
        {
            PlayerPrefs.SetInt(WelcomeDismissedPreferenceKey, 0);
            SaveModPreferencesAndConfig();
            _welcomeSuppressedUntilRestart = true;
            WriteStatus("Welcome screen reset for next game launch.");
            return true;
        }
        catch (Exception ex)
        {
            WriteStatus("Welcome screen reset failed: " + ex);
            return false;
        }
    }

    private bool ShouldBlockWelcomeBack(MainMenuUIManager main)
    {
        if (!_welcomeOpen || _welcomeAllowBack || _welcomePanel == null ||
            main.panels == null || main.panels.Count == 0 ||
            main.panels.Peek().GetInstanceID() != _welcomePanel.GetInstanceID())
            return false;
        QueueSpeech(L("Choose a welcome option to continue."));
        return true;
    }

    internal static bool AllowWelcomeBack(MainMenuUIManager main) =>
        _activeWelcomeMod?.ShouldBlockWelcomeBack(main) != true;
}

[HarmonyPatch(typeof(MainMenuUIManager), "GoBack")]
[SupportedOSPlatform("windows")]
internal static class WelcomeBackPatch
{
    [HarmonyPrefix]
    private static bool BeforeBack(MainMenuUIManager __instance) =>
        BopItAccessMod.AllowWelcomeBack(__instance);
}
