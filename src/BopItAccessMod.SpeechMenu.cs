using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private SettingsButton? _speechSettingsButton;
    private UnityAction? _speechSettingsSubmitListener;
    private int _speechSettingsPanelId;
    private long _nextSpeechSettingsProbeAt;
    private long _nextSpeechSettingsErrorAt;

    private GameObject? _speechMenuRoot;
    private Panel? _speechMenuPanel;
    private ScrollRect? _speechMenuScroll;
    private TMP_Text? _speechMenuTitle;
    private string? _speechMenuRenderedLocale;
    private readonly List<SpeechUiOption> _speechUiOptions = new();
    private int _speechMenuControlsRevision = -1;
    private string _speechMenuControlsMode = string.Empty;
    private ulong _speechMenuControlsBackendId = PrismNative.BackendIds.Invalid;
    private SettingsToggle? _speechOutputToggle;
    private SettingsToggle? _brailleOutputToggle;
    private SettingsToggle? _muteSpeechInBackgroundToggle;
    private SettingsToggle? _indexingToggle;
    private SettingsToggle? _filterCapitalisationToggle;
    private SettingsToggle? _readControlTypesToggle;
    private SettingsToggle? _sliderRangesToggle;
    private SettingsToggle? _oneOnOneFeedbackToggle;
    private SettingsToggle? _readButtonHintsToggle;
    private SettingsSlider? _hintsTypeSlider;
    private SettingsSlider? _buttonHintsDelaySlider;
    private SettingsSlider? _repeatButtonHintsSlider;
    private SettingsSlider? _repeatButtonHintsIntervalSlider;
    private SettingsSlider? _speechModeSlider;
    private SettingsToggle? _trimSilenceToggle;
    private SettingsSlider? _speechVoiceSlider;
    private SettingsSlider? _speechVolumeSlider;
    private SettingsSlider? _speechRateSlider;
    private SettingsSlider? _speechPitchSlider;
    private SettingsButton? _openUserGuideButton;
    private SettingsButton? _resetWelcomeScreenButton;
    private SettingsButton? _restoreModDefaultsButton;
    private UnityAction? _speechOutputSubmitListener;
    private UnityAction? _brailleOutputSubmitListener;
    private UnityAction? _muteSpeechInBackgroundSubmitListener;
    private UnityAction? _indexingSubmitListener;
    private UnityAction? _filterCapitalisationSubmitListener;
    private UnityAction? _readControlTypesSubmitListener;
    private UnityAction? _sliderRangesSubmitListener;
    private UnityAction? _oneOnOneFeedbackSubmitListener;
    private UnityAction? _readButtonHintsSubmitListener;
    private UnityAction? _trimSilenceSubmitListener;
    private UnityAction? _speechBackSubmitListener;
    private UnityAction? _restoreModDefaultsSubmitListener;
    private UnityAction? _openUserGuideSubmitListener;
    private UnityAction? _resetWelcomeScreenSubmitListener;
    private UnityAction<int>? _speechModeMoveListener;
    private UnityAction<int>? _hintsTypeMoveListener;
    private UnityAction<int>? _buttonHintsDelayMoveListener;
    private UnityAction<int>? _repeatButtonHintsMoveListener;
    private UnityAction<int>? _repeatButtonHintsIntervalMoveListener;
    private UnityAction<int>? _speechVoiceMoveListener;
    private UnityAction<int>? _speechVolumeMoveListener;
    private UnityAction<int>? _speechRateMoveListener;
    private UnityAction<int>? _speechPitchMoveListener;
    private bool _speechMenuOpen;
    private bool _speechMenuIntroductionPending;
    private long _speechMenuOpenedAt;
    private int _speechMenuOpenedFrame;
    private bool _speechMenuInputReady;
    private int _lastSpeechMenuRowId;
    private string? _lastSpeechMenuValue;
    private long _restoreModDefaultsConfirmUntil;
    private int _restoreModDefaultsFirstFrame;
    private int _restoreModDefaultsLastActivationFrame = -1;
    private bool _restoreModDefaultsInputReleased;
    private long _resetWelcomeScreenConfirmUntil;
    private int _resetWelcomeScreenFirstFrame;
    private int _resetWelcomeScreenLastActivationFrame = -1;
    private bool _resetWelcomeScreenInputReleased;
    private static readonly int[] ButtonHintsDelayOptions = { 0, 5, 10, 15, 30, 60 };
    private static readonly int[] RepeatButtonHintsOptions = { 0, 2, 3, 4, 5, -1 };
    private static readonly int[] RepeatButtonHintsIntervalOptions = { 15, 30, 45, 60 };
    private static string ButtonHintsDelayValue(int seconds) => seconds switch
    {
        0 => "None",
        5 => "5 seconds (May interrupt speech)",
        _ => seconds + " seconds"
    };
    private static string RepeatButtonHintsValue(int count) => count switch
    {
        0 => "Off",
        -1 => "Infinitely",
        _ => count + "x"
    };

    private static string LocalizedButtonHintsDelayValue(int seconds) => seconds switch
    {
        0 => L("None"),
        5 => LF("{0} seconds (May interrupt speech)", 5),
        _ => LF("{0} seconds", seconds)
    };

    private static string LocalizedRepeatButtonHintsValue(int count) => count switch
    {
        0 => L("Off"),
        -1 => L("Infinitely"),
        _ => LF("{0}x", count)
    };

    private static string LocalizedRepeatIntervalValue(int seconds) =>
        LF("{0} seconds", seconds);

    private void RefreshSpeechMenuLocalizedLabels()
    {
        string locale = CurrentGameLocale;
        if (string.Equals(_speechMenuRenderedLocale, locale,
            StringComparison.Ordinal))
            return;
        _speechMenuRenderedLocale = locale;
        if (_speechSettingsButton != null)
            SetSpeechRowLabel(_speechSettingsButton, "MOD SETTINGS");
        if (_speechMenuTitle != null)
            SetClonedLabel(_speechMenuTitle, L("MOD SETTINGS"));
        foreach (SpeechUiOption option in _speechUiOptions)
            SetSpeechRowLabel(option.Row, option.Label);
        UpdateSpeechMenuValues();
        _lastSpeechMenuRowId = 0;
        _lastSpeechMenuValue = null;
        _cachedButtonHintContext = null;
        _nextButtonHintContextProbeAt = 0;
    }
    private void UpdateSpeechMenuUi()
    {
        RefreshSpeechMenuLocalizedLabels();
        RefreshSpeechOutputControlRows();
        // The guide can temporarily sit above Mod Settings in the native
        // panel stack. Keep this menu's state so Back returns to its row.
        if (_guideOpen)
            return;
        if (_speechMenuOpen)
        {
            if (_speechMenuRoot == null || _speechMenuPanel == null ||
                _mainMenu == null || _mainMenu.panels == null ||
                _mainMenu.panels.Count == 0 ||
                _mainMenu.panels.Peek().GetInstanceID() != _speechMenuPanel.GetInstanceID())
            {
                if (Environment.TickCount64 - _speechMenuOpenedAt < 500)
                    WriteStatus("Mod Settings menu left the panel stack immediately after opening.");
                _speechMenuOpen = false;
                _speechMenuInputReady = false;
                CancelRestoreModDefaultsConfirmation();
                CancelResetWelcomeScreenConfirmation();
                ResetSpeechMenuFocus();
            }
            else
            {
                if (!_speechMenuInputReady && Time.frameCount > _speechMenuOpenedFrame &&
                    !IsUiSubmitHeld())
                {
                    _speechMenuInputReady = true;
                    GameObject? selected = EventSystem.current?.currentSelectedGameObject;
                    if (_speechOutputToggle != null &&
                        (selected == null || !selected.transform.IsChildOf(_speechMenuRoot.transform)))
                        EventSystem.current?.SetSelectedGameObject(_speechOutputToggle.gameObject);
                }
                if (_restoreModDefaultsConfirmUntil != 0)
                {
                    if (Environment.TickCount64 >= _restoreModDefaultsConfirmUntil ||
                        !IsRestoreModDefaultsFocused())
                        CancelRestoreModDefaultsConfirmation();
                    else if (!IsUiSubmitHeld())
                        _restoreModDefaultsInputReleased = true;
                }
                if (_resetWelcomeScreenConfirmUntil != 0)
                {
                    if (Environment.TickCount64 >= _resetWelcomeScreenConfirmUntil ||
                        !IsResetWelcomeScreenFocused())
                        CancelResetWelcomeScreenConfirmation();
                    else if (!IsUiSubmitHeld())
                        _resetWelcomeScreenInputReleased = true;
                }
                if (_speechOutputToggle != null && _speechOutputToggle.IsOn != _speechEnabled)
                    SetSpeechToggleDisplay(_speechOutputToggle, _speechEnabled);
                if (_brailleOutputToggle != null &&
                    _brailleOutputToggle.IsOn != _brailleOutputEnabled)
                    SetSpeechToggleDisplay(_brailleOutputToggle,
                        _brailleOutputEnabled);
                if (_muteSpeechInBackgroundToggle != null &&
                    _muteSpeechInBackgroundToggle.IsOn != _muteSpeechInBackground)
                    SetSpeechToggleDisplay(_muteSpeechInBackgroundToggle,
                        _muteSpeechInBackground);
                if (_indexingToggle != null && _indexingToggle.IsOn != _indexingEnabled)
                    SetSpeechToggleDisplay(_indexingToggle, _indexingEnabled);
                if (_filterCapitalisationToggle != null &&
                    _filterCapitalisationToggle.IsOn != _filterCapitalisationEnabled)
                    SetSpeechToggleDisplay(_filterCapitalisationToggle,
                        _filterCapitalisationEnabled);
                if (_readControlTypesToggle != null &&
                    _readControlTypesToggle.IsOn != _readControlTypesEnabled)
                    SetSpeechToggleDisplay(_readControlTypesToggle,
                        _readControlTypesEnabled);
                if (_sliderRangesToggle != null &&
                    _sliderRangesToggle.IsOn != _sliderRangesEnabled)
                    SetSpeechToggleDisplay(_sliderRangesToggle, _sliderRangesEnabled);
                if (_oneOnOneFeedbackToggle != null &&
                    _oneOnOneFeedbackToggle.IsOn != _oneOnOneFeedbackEnabled)
                    SetSpeechToggleDisplay(_oneOnOneFeedbackToggle,
                        _oneOnOneFeedbackEnabled);
                if (_readButtonHintsToggle != null &&
                    _readButtonHintsToggle.IsOn != _readButtonHintsEnabled)
                    SetSpeechToggleDisplay(_readButtonHintsToggle,
                        _readButtonHintsEnabled);
                if (_trimSilenceToggle != null && _trimSilenceToggle.IsOn != _trimSilence)
                    SetSpeechToggleDisplay(_trimSilenceToggle, _trimSilence);
                ScrollSelectedRowIntoView(_speechMenuScroll);
            }
        }

        long now = Environment.TickCount64;
        if (now < _nextSpeechSettingsProbeAt)
            return;
        _nextSpeechSettingsProbeAt = now + 250;

        SettingsPanel? settings = _settingsPanel;
        if (settings == null)
            settings = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
        if (settings == null || !settings.IsVisible || !settings.gameObject.activeInHierarchy)
            return;

        int settingsId = settings.GetInstanceID();
        if (_speechSettingsButton != null && _speechSettingsPanelId == settingsId)
            return;

        try
        {
            AddSpeechSettingsButton(settings);
        }
        catch (Exception ex)
        {
            if (now >= _nextSpeechSettingsErrorAt)
            {
                WriteStatus("MOD SETTINGS row could not be added: " + ex);
                _nextSpeechSettingsErrorAt = now + 5000;
            }
        }
    }

    private void AddSpeechSettingsButton(SettingsPanel settings)
    {
        SettingsButton? template = settings.controls;
        if (template == null || template.transform.parent == null)
            throw new InvalidOperationException("The native Controls settings row is unavailable.");

        Transform content = template.transform.parent;
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, content, false);
        clone.SetActive(false);
        try
        {
            clone.name = "BopItAccess Mod Settings";
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
            SettingsButton? button = clone.GetComponent<SettingsButton>();
            if (button == null)
                throw new InvalidOperationException("The cloned Controls row lost SettingsButton.");

            SetSpeechRowLabel(button, "MOD SETTINGS");
            button.Submitted = new UnityEvent();
            button.SliderMoved = new UnityEvent<int>();
            button.SetValue(string.Empty);
            _speechSettingsSubmitListener ??= (UnityAction)OpenSpeechMenu;
            button.Submitted.AddListener(_speechSettingsSubmitListener);

            _speechSettingsButton = button;
            _speechSettingsPanelId = settings.GetInstanceID();
            clone.SetActive(true);
            if (content.GetComponent<RectTransform>() is RectTransform contentRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            if (_settingsWasVisible)
                _settingsOptions = CreateSettingsOptions(settings);
            WriteStatus("Added MOD SETTINGS menu below Controls in Settings.");
        }
        catch
        {
            _speechSettingsButton = null;
            _speechSettingsPanelId = 0;
            UnityEngine.Object.Destroy(clone);
            throw;
        }
    }

    private void OpenSpeechMenu()
    {
        if (_speechMenuOpen)
            return;
        SettingsPanel? settings = _settingsPanel;
        MainMenuUIManager? main = _mainMenu;
        if (settings == null || main == null || main.panels == null ||
            !settings.IsVisible || _speechSettingsButton == null)
            return;

        bool settingsHidden = false;
        bool pushed = false;
        try
        {
            RefreshSapiVoices();
            if (_speechMenuPanel == null || _speechMenuRoot == null)
                BuildSpeechMenu(settings, main);
            if (_speechMenuPanel == null || _speechMenuRoot == null)
                return;

            RefreshSpeechOutputControlRows(force: true);
            UpdateSpeechMenuValues();
            // GoBack pops the top panel and shows the previous one, just as it
            // does when returning from the game's Controls panel.
            settings.lastSelectedButton = _speechSettingsButton.gameObject;
            settings.Hide();
            settingsHidden = true;
            _speechMenuRoot.SetActive(true);
            // Panel.Hide remembers the selected row. If that row was BACK,
            // reopening with Enter can submit BACK during the same UI event
            // and close this panel before it has a visible frame.
            _speechMenuPanel.lastSelectedButton = null;
            _speechMenuPanel.firstSelectedButton = _speechOutputToggle?.gameObject;
            _speechMenuOpenedFrame = Time.frameCount;
            _speechMenuInputReady = false;
            _speechMenuPanel.Show();
            main.panels.Push(_speechMenuPanel);
            pushed = true;
            // The opening submit can be sent again to a newly selected row.
            // Leave focus empty until that press is released on a later frame.
            EventSystem.current?.SetSelectedGameObject(null);
            _speechMenuOpen = true;
            _speechMenuIntroductionPending = true;
            _speechMenuOpenedAt = Environment.TickCount64;
            CancelRestoreModDefaultsConfirmation();
            CancelResetWelcomeScreenConfirmation();
            ResetSpeechMenuFocus();
            WriteStatus("Opened Mod Settings menu.");
        }
        catch (Exception ex)
        {
            WriteStatus("Mod Settings menu could not be opened: " + ex);
            if (pushed && main.panels.Count > 0 && _speechMenuPanel != null &&
                main.panels.Peek().GetInstanceID() == _speechMenuPanel.GetInstanceID())
                main.panels.Pop();
            if (_speechMenuRoot != null)
                _speechMenuRoot.SetActive(false);
            if (settingsHidden)
                settings.Show();
            _speechMenuOpen = false;
            _speechMenuInputReady = false;
            CancelRestoreModDefaultsConfirmation();
            CancelResetWelcomeScreenConfirmation();
        }
    }

    private bool IsUiSubmitHeld()
    {
        if (Mouse.current?.leftButton.isPressed == true)
            return true;
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        InputAction? submit = module?.submit?.action;
        if (submit != null && submit.enabled && submit.IsPressed())
            return true;

        // The game's ControlRow uses PlayerInput's Submit action. Check it
        // too so both the native and EventSystem routes must be released.
        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        InputRebindingManager? manager = main?.controlsPanel?
            .GetComponentInChildren<InputRebindingManager>(true);
        submit = manager?.playerInput?.actions?.FindAction("Submit", false);
        return submit != null && submit.enabled && submit.IsPressed();
    }

    private void BuildSpeechMenu(SettingsPanel settings, MainMenuUIManager main)
    {
        Panel? controls = main.controlsPanel;
        RectTransform? sourceRect = controls?.GetComponent<RectTransform>();
        Transform? sourceLayout = controls?.transform.Find("Layout");
        if (controls == null || sourceRect == null || sourceLayout == null ||
            controls.transform.parent == null)
            throw new InvalidOperationException("The native Controls panel layout is unavailable.");

        GameObject root = new("BopItAccess Mod Settings Menu",
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
            Panel speechPanel = root.AddComponent<Panel>();
            speechPanel.canvasGroup = group;

            GameObject layout = UnityEngine.Object.Instantiate(sourceLayout.gameObject,
                root.transform, false);
            Transform? table = layout.transform.Find("Table");
            // Controls may already have the mod's scrolling viewport from an
            // earlier visit. In that case Content has moved one level down.
            Transform? content = table?.Find("Content") ??
                table?.Find("BopItAccess Controls Viewport/Content");
            TMP_Text? title = table?.Find("Title")?.GetComponent<TMP_Text>();
            if (table == null || content == null || title == null)
                throw new InvalidOperationException("The cloned Controls table is incomplete.");
            SetClonedLabel(title, L("MOD SETTINGS"));
            _speechMenuTitle = title;
            _speechMenuRenderedLocale = CurrentGameLocale;

            // Only visual pieces of the Controls panel are cloned. Its native
            // rebinding manager stays on the original panel, and its copied
            // binding rows are disabled before this panel can become active.
            for (int i = 0; i < content.childCount; i++)
                content.GetChild(i).gameObject.SetActive(false);

            Transform? prompt = controls.transform.Find("ControlPrompt");
            if (prompt != null)
                UnityEngine.Object.Instantiate(prompt.gameObject, root.transform, false);

            _speechUiOptions.Clear();
            _speechOutputToggle = AddSpeechToggle(settings.vibration, content,
                "SPEECH OUTPUT",
                _speechOutputSubmitListener ??= (UnityAction)OnSpeechOutputSubmitted,
                _speechEnabled);
            _brailleOutputToggle = AddSpeechToggle(settings.vibration, content,
                "BRAILLE OUTPUT",
                _brailleOutputSubmitListener ??= (UnityAction)OnBrailleOutputSubmitted,
                _brailleOutputEnabled);
            _muteSpeechInBackgroundToggle = AddSpeechToggle(settings.vibration, content,
                "MUTE SPEECH IN BACKGROUND",
                _muteSpeechInBackgroundSubmitListener ??=
                    (UnityAction)OnMuteSpeechInBackgroundSubmitted,
                _muteSpeechInBackground);
            _indexingToggle = AddSpeechToggle(settings.vibration, content,
                "SPEAK MENU INDEXES",
                _indexingSubmitListener ??= (UnityAction)OnIndexingSubmitted,
                _indexingEnabled);
            _filterCapitalisationToggle = AddSpeechToggle(settings.vibration, content,
                "FORMAT SPEECH",
                _filterCapitalisationSubmitListener ??=
                    (UnityAction)OnFilterCapitalisationSubmitted,
                _filterCapitalisationEnabled);
            _readControlTypesToggle = AddSpeechToggle(settings.vibration, content,
                "SPEAK CONTROL TYPES",
                _readControlTypesSubmitListener ??=
                    (UnityAction)OnReadControlTypesSubmitted,
                _readControlTypesEnabled);
            _sliderRangesToggle = AddSpeechToggle(settings.vibration, content,
                "SLIDER RANGES",
                _sliderRangesSubmitListener ??= (UnityAction)OnSliderRangesSubmitted,
                _sliderRangesEnabled);
            _oneOnOneFeedbackToggle = AddSpeechToggle(settings.vibration, content,
                "ONE-ON-ONE FEEDBACK",
                _oneOnOneFeedbackSubmitListener ??=
                    (UnityAction)OnOneOnOneFeedbackSubmitted,
                _oneOnOneFeedbackEnabled);
            _hintsTypeSlider = AddSpeechSlider(settings.resolution, content,
                "HINTS TYPE");
            _readButtonHintsToggle = AddSpeechToggle(settings.vibration, content,
                "AUTO-SPEAK BUTTON HINTS",
                _readButtonHintsSubmitListener ??=
                    (UnityAction)OnReadButtonHintsSubmitted,
                _readButtonHintsEnabled);
            _buttonHintsDelaySlider = AddSpeechSlider(settings.resolution, content,
                "BUTTON HINTS DELAY");
            _repeatButtonHintsSlider = AddSpeechSlider(settings.resolution, content,
                "REPEAT BUTTON HINTS");
            _repeatButtonHintsIntervalSlider = AddSpeechSlider(settings.resolution, content,
                "REPEAT INTERVAL");
            _speechModeSlider = AddSpeechSlider(settings.resolution, content,
                "OUTPUT MODE");
            _trimSilenceToggle = null;
            if (TrimSilenceExperimentAvailable)
                _trimSilenceToggle = AddSpeechToggle(settings.vibration, content,
                    "TRIM SILENCE",
                    _trimSilenceSubmitListener ??= (UnityAction)OnTrimSilenceSubmitted,
                    _trimSilence);
            _speechVoiceSlider = AddSpeechSlider(settings.resolution, content,
                "VOICE");
            _speechVolumeSlider = AddSpeechSlider(settings.resolution, content,
                "VOLUME");
            _speechRateSlider = AddSpeechSlider(settings.resolution, content,
                "RATE");
            _speechPitchSlider = AddSpeechSlider(settings.resolution, content,
                "PITCH");
            _openUserGuideButton = AddSpeechButton(settings.controls, content,
                "OPEN USER'S GUIDE",
                _openUserGuideSubmitListener ??=
                    (UnityAction)OnOpenUserGuideSubmitted);
            _resetWelcomeScreenButton = AddSpeechButton(settings.controls, content,
                "RESET WELCOME SCREEN",
                _resetWelcomeScreenSubmitListener ??=
                    (UnityAction)OnResetWelcomeScreenSubmitted);
            _restoreModDefaultsButton = AddSpeechButton(settings.controls, content,
                "RESTORE MOD DEFAULTS",
                _restoreModDefaultsSubmitListener ??=
                    (UnityAction)OnRestoreModDefaultsSubmitted);
            SettingsButton back = AddSpeechButton(settings.controls, content,
                "BACK", _speechBackSubmitListener ??= (UnityAction)OnSpeechBackSubmitted);

            _speechUiOptions.Add(new("SPEECH OUTPUT", "toggle", _speechOutputToggle,
                () => _speechEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("BRAILLE OUTPUT", "toggle", _brailleOutputToggle,
                () => _brailleOutputEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("MUTE SPEECH IN BACKGROUND", "toggle",
                _muteSpeechInBackgroundToggle,
                () => _muteSpeechInBackground ? "On" : "Off"));
            _speechUiOptions.Add(new("SPEAK MENU INDEXES", "toggle", _indexingToggle,
                () => _indexingEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("FORMAT SPEECH", "toggle",
                _filterCapitalisationToggle,
                () => _filterCapitalisationEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("SPEAK CONTROL TYPES", "toggle",
                _readControlTypesToggle,
                () => _readControlTypesEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("SLIDER RANGES", "toggle", _sliderRangesToggle,
                () => _sliderRangesEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("ONE-ON-ONE FEEDBACK", "toggle",
                _oneOnOneFeedbackToggle,
                () => _oneOnOneFeedbackEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("HINTS TYPE", "slider", _hintsTypeSlider,
                () => _hintsType));
            _speechUiOptions.Add(new("AUTO-SPEAK BUTTON HINTS", "toggle",
                _readButtonHintsToggle,
                () => _readButtonHintsEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("BUTTON HINTS DELAY", "slider",
                _buttonHintsDelaySlider,
                () => ButtonHintsDelayValue(_buttonHintsDelaySeconds)));
            _speechUiOptions.Add(new("REPEAT BUTTON HINTS", "slider",
                _repeatButtonHintsSlider,
                () => RepeatButtonHintsValue(_repeatButtonHintsCount)));
            _speechUiOptions.Add(new("REPEAT INTERVAL", "slider",
                _repeatButtonHintsIntervalSlider,
                () => _repeatButtonHintsIntervalSeconds + " seconds"));
            _speechUiOptions.Add(new("OUTPUT MODE", "slider", _speechModeSlider,
                () => _outputMode));
            if (TrimSilenceExperimentAvailable && _trimSilenceToggle != null)
                _speechUiOptions.Add(new("TRIM SILENCE", "toggle", _trimSilenceToggle,
                    () => _trimSilence ? "On" : "Off"));
            _speechUiOptions.Add(new("VOICE", "slider", _speechVoiceSlider,
                ReadCurrentSpeechVoiceName));
            _speechUiOptions.Add(new("VOLUME", "slider", _speechVolumeSlider,
                () => CurrentSpeechVolume() + "%"));
            _speechUiOptions.Add(new("RATE", "slider", _speechRateSlider,
                () => CurrentSpeechRate().ToString()));
            _speechUiOptions.Add(new("PITCH", "slider", _speechPitchSlider,
                () => CurrentSpeechPitch().ToString()));
            _speechUiOptions.Add(new("OPEN USER'S GUIDE", "button",
                _openUserGuideButton, () => null));
            _speechUiOptions.Add(new("RESET WELCOME SCREEN", "button",
                _resetWelcomeScreenButton,
                () => _resetWelcomeScreenConfirmUntil != 0
                    ? "Press again to confirm" : null));
            _speechUiOptions.Add(new("RESTORE MOD DEFAULTS", "button",
                _restoreModDefaultsButton,
                () => _restoreModDefaultsConfirmUntil != 0
                    ? "Press again to confirm" : null));
            _speechUiOptions.Add(new("BACK", "button", back, () => null));

            speechPanel.firstSelectedButton = _speechOutputToggle.gameObject;
            speechPanel.lastSelectedButton = null;
            RectTransform? tableRect = table.GetComponent<RectTransform>();
            RectTransform? contentRect = content.GetComponent<RectTransform>();
            if (tableRect != null && contentRect != null)
            {
                _speechMenuScroll = tableRect.GetComponent<ScrollRect>();
                if (_speechMenuScroll == null)
                    _speechMenuScroll = AddSpeechViewport(tableRect, contentRect,
                        "BopItAccess Speech Viewport");
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                if (_speechMenuScroll != null)
                    _speechMenuScroll.verticalNormalizedPosition = 1f;
            }

            _speechMenuRoot = root;
            _speechMenuPanel = speechPanel;
            RefreshSpeechOutputControlRows(force: true);
            WriteStatus($"Built Mod Settings submenu with {_speechUiOptions.Count} native-style rows.");
        }
        catch
        {
            UnityEngine.Object.Destroy(root);
            _speechUiOptions.Clear();
            _speechOutputToggle = null;
            _brailleOutputToggle = null;
            _muteSpeechInBackgroundToggle = null;
            _indexingToggle = null;
            _filterCapitalisationToggle = null;
            _readControlTypesToggle = null;
            _sliderRangesToggle = null;
            _oneOnOneFeedbackToggle = null;
            _readButtonHintsToggle = null;
            _hintsTypeSlider = null;
            _buttonHintsDelaySlider = null;
            _repeatButtonHintsSlider = null;
            _repeatButtonHintsIntervalSlider = null;
            _speechModeSlider = null;
            _trimSilenceToggle = null;
            _speechVoiceSlider = null;
            _speechVolumeSlider = null;
            _speechRateSlider = null;
            _speechPitchSlider = null;
            _speechMenuTitle = null;
            _openUserGuideButton = null;
            _resetWelcomeScreenButton = null;
            _restoreModDefaultsButton = null;
            _speechMenuScroll = null;
            throw;
        }
    }

    private SettingsToggle AddSpeechToggle(SettingsToggle? source, Transform parent,
        string label, UnityAction listener, bool enabled)
    {
        if (source == null)
            throw new InvalidOperationException("The native toggle row is unavailable.");
        GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
        clone.SetActive(false);
        clone.name = "BopItAccess " + label;
        SettingsToggle? row = clone.GetComponent<SettingsToggle>();
        if (row == null)
            throw new InvalidOperationException("The cloned toggle lost SettingsToggle.");
        SetSpeechRowLabel(row, label);
        row.Submitted = new UnityEvent();
        row.ValueChanged = new UnityEvent<bool>();
        row.Submitted.AddListener(listener);
        SetSpeechToggleDisplay(row, enabled);
        clone.SetActive(true);
        return row;
    }

    private SettingsSlider AddSpeechSlider(SettingsSlider? source, Transform parent,
        string label)
    {
        if (source == null)
            throw new InvalidOperationException("The native slider row is unavailable.");
        GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
        clone.SetActive(false);
        clone.name = "BopItAccess " + label;
        SettingsSlider? row = clone.GetComponent<SettingsSlider>();
        if (row == null)
            throw new InvalidOperationException("The cloned slider lost SettingsSlider.");
        SetSpeechRowLabel(row, label);
        row.SliderMoved = new UnityEvent<int>();
        row.Submitted = new UnityEvent();
        UnityAction<int> listener = label switch
        {
            "OUTPUT MODE" => _speechModeMoveListener ??= (UnityAction<int>)OnSpeechModeMoved,
            "HINTS TYPE" => _hintsTypeMoveListener ??= (UnityAction<int>)OnHintsTypeMoved,
            "BUTTON HINTS DELAY" => _buttonHintsDelayMoveListener ??=
                (UnityAction<int>)OnButtonHintsDelayMoved,
            "REPEAT BUTTON HINTS" => _repeatButtonHintsMoveListener ??=
                (UnityAction<int>)OnRepeatButtonHintsMoved,
            "REPEAT INTERVAL" => _repeatButtonHintsIntervalMoveListener ??=
                (UnityAction<int>)OnRepeatButtonHintsIntervalMoved,
            "VOICE" => _speechVoiceMoveListener ??= (UnityAction<int>)OnSpeechVoiceMoved,
            "VOLUME" => _speechVolumeMoveListener ??= (UnityAction<int>)OnSpeechVolumeMoved,
            "RATE" => _speechRateMoveListener ??= (UnityAction<int>)OnSpeechRateMoved,
            "PITCH" => _speechPitchMoveListener ??= (UnityAction<int>)OnSpeechPitchMoved,
            _ => throw new ArgumentOutOfRangeException(nameof(label), label,
                "No movement handler was registered for this Speech slider.")
        };
        row.SliderMoved.AddListener(listener);
        clone.SetActive(true);
        return row;
    }

    private SettingsButton AddSpeechButton(SettingsButton? source, Transform parent,
        string label, UnityAction listener)
    {
        if (source == null)
            throw new InvalidOperationException("The native button row is unavailable.");
        GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
        clone.SetActive(false);
        clone.name = "BopItAccess " + label;
        SettingsButton? row = clone.GetComponent<SettingsButton>();
        if (row == null)
            throw new InvalidOperationException("The cloned button lost SettingsButton.");
        SetSpeechRowLabel(row, label);
        row.Submitted = new UnityEvent();
        row.SliderMoved = new UnityEvent<int>();
        row.SetValue(string.Empty);
        row.Submitted.AddListener(listener);
        clone.SetActive(true);
        return row;
    }

    private static void SetSpeechRowLabel(SettingsRow row, string label)
    {
        TMP_Text? regular = FindFpsLabel(row.DefaultContainer, row.ValueText);
        TMP_Text? active = FindFpsLabel(row.ActiveContainer, row.ActiveValueText);
        if (regular == null || active == null)
            throw new InvalidOperationException("A cloned settings row has no visible labels.");
        SetClonedLabel(regular, L(label));
        SetClonedLabel(active, L(label));
        if (row.ValueText != null)
            DisableFpsValueLocalization(row.ValueText);
        if (row.ActiveValueText != null)
            DisableFpsValueLocalization(row.ActiveValueText);
    }

    private static void SetSpeechToggleDisplay(SettingsToggle row, bool enabled)
    {
        row.SetValue(enabled);
        string value = L(enabled ? "On" : "Off");
        if (row.ValueText != null)
            row.ValueText.text = value;
        if (row.ActiveValueText != null)
            row.ActiveValueText.text = value;
    }

    private static ScrollRect AddSpeechViewport(RectTransform table,
        RectTransform content, string name)
    {
        Vector2 anchorMin = content.anchorMin;
        Vector2 anchorMax = content.anchorMax;
        Vector2 pivot = content.pivot;
        Vector2 position = content.anchoredPosition;
        Vector2 size = content.sizeDelta;
        int sibling = content.GetSiblingIndex();

        GameObject viewport = new(name,
            new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.SetParent(table, false);
        viewportRect.SetSiblingIndex(sibling);
        viewportRect.anchorMin = anchorMin;
        viewportRect.anchorMax = anchorMax;
        viewportRect.pivot = pivot;
        viewportRect.anchoredPosition = position;
        viewportRect.sizeDelta = size;
        viewport.AddComponent<RectMask2D>();

        content.SetParent(viewportRect, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        ContentSizeFitter? fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = table.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.inertia = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return scroll;
    }

    private static void ScrollSelectedRowIntoView(ScrollRect? scroll)
    {
        if (scroll == null || scroll.viewport == null || scroll.content == null)
            return;
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(scroll.content))
            return;

        SettingsRow? row = selected.GetComponentInParent<SettingsRow>();
        Transform item = row != null && row.transform.IsChildOf(scroll.content)
            ? row.transform : selected.transform;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            scroll.viewport, item);
        Rect viewport = scroll.viewport.rect;
        float overflow = bounds.min.y < viewport.yMin
            ? bounds.min.y - viewport.yMin
            : bounds.max.y > viewport.yMax
                ? bounds.max.y - viewport.yMax : 0f;
        if (Mathf.Abs(overflow) < 0.5f)
            return;
        float height = scroll.content.rect.height - viewport.height;
        if (height > 0f)
            scroll.verticalNormalizedPosition = Mathf.Clamp01(
                scroll.verticalNormalizedPosition + overflow / height);
    }

    private void RefreshSpeechOutputControlRows(bool force = false)
    {
        if (_speechMenuRoot == null || _speechModeSlider == null ||
            _speechVoiceSlider == null || _speechVolumeSlider == null ||
            _speechRateSlider == null || _speechPitchSlider == null)
            return;

        PrismSpeechControlSnapshot? snapshot;
        lock (_speechLock)
            snapshot = _prismSpeechControlSnapshot;
        string mode = _outputMode;
        int revision = snapshot?.Revision ?? -1;
        if (!force && revision == _speechMenuControlsRevision &&
            string.Equals(mode, _speechMenuControlsMode, StringComparison.Ordinal))
            return;
        _speechMenuControlsRevision = revision;
        _speechMenuControlsMode = mode;

        // A mode switch is resolved on the worker. Until its new snapshot
        // arrives, avoid presenting controls for the previous output engine.
        bool current = snapshot != null &&
            string.Equals(snapshot.SelectedMode, mode, StringComparison.OrdinalIgnoreCase);
        ulong backendId = current ? snapshot!.BackendId : PrismNative.BackendIds.Invalid;
        PrismNative.BackendFeature features = current ? snapshot!.Features : 0;
        _speechMenuControlsBackendId = backendId;
        if (backendId == PrismNative.BackendIds.OneCore)
            _oneCoreVoices = snapshot!.OneCoreVoices.ToList();

        bool adjustable = backendId == PrismNative.BackendIds.Sapi ||
            backendId == PrismNative.BackendIds.OneCore;
        string prefix = backendId == PrismNative.BackendIds.OneCore ? "ONECORE" : "SAPI";
        bool voice = adjustable && (features &
            (PrismNative.BackendFeature.SupportsSetVoice |
             PrismNative.BackendFeature.SupportsGetVoice |
             PrismNative.BackendFeature.SupportsCountVoices |
             PrismNative.BackendFeature.SupportsGetVoiceName)) ==
            (PrismNative.BackendFeature.SupportsSetVoice |
             PrismNative.BackendFeature.SupportsGetVoice |
             PrismNative.BackendFeature.SupportsCountVoices |
             PrismNative.BackendFeature.SupportsGetVoiceName) &&
            (backendId == PrismNative.BackendIds.OneCore
                ? _oneCoreVoices.Count > 1 : _sapiVoices.Count > 1);
        bool volume = adjustable &&
            (features & PrismNative.BackendFeature.SupportsSetVolume) != 0;
        bool rate = adjustable &&
            (features & PrismNative.BackendFeature.SupportsSetRate) != 0;
        bool pitch = adjustable &&
            (features & PrismNative.BackendFeature.SupportsSetPitch) != 0;

        SettingsRow? selectedRow = EventSystem.current?.currentSelectedGameObject?
            .GetComponentInParent<SettingsRow>();
        bool selectedControlWillHide =
            (selectedRow == _speechVoiceSlider && !voice) ||
            (selectedRow == _speechVolumeSlider && !volume) ||
            (selectedRow == _speechRateSlider && !rate) ||
            (selectedRow == _speechPitchSlider && !pitch);
        bool selectedControlWillRefresh = selectedRow != null &&
            (selectedRow == _speechVoiceSlider || selectedRow == _speechVolumeSlider ||
             selectedRow == _speechRateSlider || selectedRow == _speechPitchSlider);

        _speechUiOptions.RemoveAll(option =>
            option.Row == _speechVoiceSlider || option.Row == _speechVolumeSlider ||
            option.Row == _speechRateSlider || option.Row == _speechPitchSlider);
        int insertion = _speechUiOptions.FindIndex(option => option.Row == _speechModeSlider) + 1;
        if (insertion == 0)
            insertion = _speechUiOptions.Count;
        if (voice)
            _speechUiOptions.Insert(insertion++, new("VOICE", "slider",
                _speechVoiceSlider, ReadCurrentSpeechVoiceName));
        if (volume)
            _speechUiOptions.Insert(insertion++, new("VOLUME", "slider",
                _speechVolumeSlider, () => CurrentSpeechVolume() + "%"));
        if (rate)
            _speechUiOptions.Insert(insertion++, new("RATE", "slider",
                _speechRateSlider, () => CurrentSpeechRate().ToString()));
        if (pitch)
            _speechUiOptions.Insert(insertion, new("PITCH", "slider",
                _speechPitchSlider, () => CurrentSpeechPitch().ToString()));

        _speechVoiceSlider.gameObject.SetActive(voice);
        _speechVolumeSlider.gameObject.SetActive(volume);
        _speechRateSlider.gameObject.SetActive(rate);
        _speechPitchSlider.gameObject.SetActive(pitch);
        if (voice)
            SetSpeechRowLabel(_speechVoiceSlider, "VOICE");
        if (volume)
            SetSpeechRowLabel(_speechVolumeSlider, "VOLUME");
        if (rate)
            SetSpeechRowLabel(_speechRateSlider, "RATE");
        if (pitch)
            SetSpeechRowLabel(_speechPitchSlider, "PITCH");

        UpdateSpeechMenuValues();
        if (selectedControlWillHide)
            EventSystem.current?.SetSelectedGameObject(_speechModeSlider.gameObject);
        if (selectedControlWillHide || selectedControlWillRefresh)
            ResetSpeechMenuFocus();
        _cachedButtonHintContext = null;
        _nextButtonHintContextProbeAt = 0;
        if (_speechMenuScroll?.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_speechMenuScroll.content);
        WriteStatus("Mod Settings voice controls now follow " +
            (adjustable ? prefix : "the selected Prism output") + ".");
    }

    private int CurrentSpeechVolume() =>
        _speechMenuControlsBackendId == PrismNative.BackendIds.OneCore
            ? _oneCoreVolume : _sapiVolume;
    private int CurrentSpeechRate() =>
        _speechMenuControlsBackendId == PrismNative.BackendIds.OneCore
            ? _oneCoreRate : _sapiRate;
    private int CurrentSpeechPitch() =>
        _speechMenuControlsBackendId == PrismNative.BackendIds.OneCore
            ? _oneCorePitch : _sapiPitch;

    private string ReadCurrentSpeechVoiceName()
    {
        if (_speechMenuControlsBackendId != PrismNative.BackendIds.OneCore)
            return ReadCurrentSapiVoiceName();
        foreach (SpeechVoiceOption voice in _oneCoreVoices)
        {
            if (string.Equals(voice.Id, _oneCoreVoiceId,
                    StringComparison.OrdinalIgnoreCase))
                return voice.Id.Length == 0 ? L(voice.Name) : voice.Name;
        }
        return L("System default");
    }

    private void UpdateSpeechMenuValues()
    {
        if (_speechOutputToggle != null)
            SetSpeechToggleDisplay(_speechOutputToggle, _speechEnabled);
        if (_brailleOutputToggle != null)
            SetSpeechToggleDisplay(_brailleOutputToggle, _brailleOutputEnabled);
        if (_muteSpeechInBackgroundToggle != null)
            SetSpeechToggleDisplay(_muteSpeechInBackgroundToggle,
                _muteSpeechInBackground);
        if (_indexingToggle != null)
            SetSpeechToggleDisplay(_indexingToggle, _indexingEnabled);
        if (_filterCapitalisationToggle != null)
            SetSpeechToggleDisplay(_filterCapitalisationToggle,
                _filterCapitalisationEnabled);
        if (_readControlTypesToggle != null)
            SetSpeechToggleDisplay(_readControlTypesToggle, _readControlTypesEnabled);
        if (_sliderRangesToggle != null)
            SetSpeechToggleDisplay(_sliderRangesToggle, _sliderRangesEnabled);
        if (_oneOnOneFeedbackToggle != null)
            SetSpeechToggleDisplay(_oneOnOneFeedbackToggle,
                _oneOnOneFeedbackEnabled);
        if (_readButtonHintsToggle != null)
            SetSpeechToggleDisplay(_readButtonHintsToggle, _readButtonHintsEnabled);
        _hintsTypeSlider?.SetValue(L(_hintsType));
        _buttonHintsDelaySlider?.SetValue(
            LocalizedButtonHintsDelayValue(_buttonHintsDelaySeconds));
        _repeatButtonHintsSlider?.SetValue(
            LocalizedRepeatButtonHintsValue(_repeatButtonHintsCount));
        _repeatButtonHintsIntervalSlider?.SetValue(
            LocalizedRepeatIntervalValue(_repeatButtonHintsIntervalSeconds));
        _speechModeSlider?.SetValue(L(_outputMode));
        if (_trimSilenceToggle != null)
            SetSpeechToggleDisplay(_trimSilenceToggle, _trimSilence);
        _speechVoiceSlider?.SetValue(ReadCurrentSpeechVoiceName());
        _speechVolumeSlider?.SetValue(CurrentSpeechVolume() + "%");
        _speechRateSlider?.SetValue(CurrentSpeechRate().ToString());
        _speechPitchSlider?.SetValue(CurrentSpeechPitch().ToString());
    }

    private string ReadCurrentSapiVoiceName()
    {
        foreach (SpeechVoiceOption voice in _sapiVoices)
        {
            if (string.Equals(voice.Id, _sapiVoiceId, StringComparison.OrdinalIgnoreCase))
                return voice.Id.Length == 0 ? L(voice.Name) : voice.Name;
        }
        return _sapiVoices.Count == 0 ? L("No SAPI voices installed") :
            L(_sapiVoices[0].Name);
    }

    private void OnSpeechOutputSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
        {
            WriteStatus("Ignored a SPEECH OUTPUT submit from the input that opened the menu.");
            return;
        }
        SetSpeechEnabledFromMenu(!_speechEnabled);
        // SettingsToggle.Start also registers its native Toggle listener on
        // Submitted. It runs after this callback and updates the visual row.
        // UpdateSpeechMenuUi reconciles it if that listener is delayed.
    }

    private void OnIndexingSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetIndexingFromMenu(!_indexingEnabled);
        // Keep the cloned native toggle in sync if its own visual listener
        // runs after this callback.
    }

    private void OnFilterCapitalisationSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetFilterCapitalisationFromMenu(!_filterCapitalisationEnabled);
    }

    private void OnBrailleOutputSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetBrailleOutputFromMenu(!_brailleOutputEnabled);
    }

    private void OnMuteSpeechInBackgroundSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetMuteSpeechInBackgroundFromMenu(!_muteSpeechInBackground);
    }

    private void OnReadControlTypesSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetReadControlTypesFromMenu(!_readControlTypesEnabled);
    }

    private void OnSliderRangesSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetSliderRangesFromMenu(!_sliderRangesEnabled);
    }

    private void OnOneOnOneFeedbackSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetOneOnOneFeedbackFromMenu(!_oneOnOneFeedbackEnabled);
    }

    private void OnReadButtonHintsSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        SetReadButtonHintsFromMenu(!_readButtonHintsEnabled);
    }

    private void OnTrimSilenceSubmitted()
    {
        if (!TrimSilenceExperimentAvailable || !_speechMenuOpen ||
            !_speechMenuInputReady)
            return;
        SetTrimSilenceFromMenu(!_trimSilence);
        // SettingsToggle.Start also registers its native visual listener.
        // UpdateSpeechMenuUi reconciles the row with the saved setting.
    }

    private void OnSpeechModeMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.FindIndex(OutputModes,
            mode => string.Equals(mode, _outputMode, StringComparison.OrdinalIgnoreCase));
        int next = Math.Clamp(Math.Max(0, current) + direction, 0,
            OutputModes.Length - 1);
        if (next == current)
            return;
        SetOutputModeFromMenu(OutputModes[next]);
        _speechModeSlider?.SetValue(L(_outputMode));
    }

    private void OnHintsTypeMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.FindIndex(HintsTypes, option =>
            string.Equals(option, _hintsType, StringComparison.OrdinalIgnoreCase));
        int next = Math.Clamp(Math.Max(0, current) + direction, 0,
            HintsTypes.Length - 1);
        if (next == current)
            return;
        SetHintsTypeFromMenu(HintsTypes[next]);
        _hintsTypeSlider?.SetValue(L(_hintsType));
    }

    private void OnRepeatButtonHintsMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.IndexOf(RepeatButtonHintsOptions, _repeatButtonHintsCount);
        int next = Math.Clamp(Math.Max(0, current) + direction, 0,
            RepeatButtonHintsOptions.Length - 1);
        if (next == current)
            return;
        SetRepeatButtonHintsFromMenu(RepeatButtonHintsOptions[next]);
        _repeatButtonHintsSlider?.SetValue(
            LocalizedRepeatButtonHintsValue(_repeatButtonHintsCount));
    }

    private void OnButtonHintsDelayMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.IndexOf(ButtonHintsDelayOptions, _buttonHintsDelaySeconds);
        int next = Math.Clamp(Math.Max(0, current) + direction, 0,
            ButtonHintsDelayOptions.Length - 1);
        if (next == current)
            return;
        SetButtonHintsDelayFromMenu(ButtonHintsDelayOptions[next]);
        _buttonHintsDelaySlider?.SetValue(
            LocalizedButtonHintsDelayValue(_buttonHintsDelaySeconds));
    }

    private void OnRepeatButtonHintsIntervalMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.IndexOf(RepeatButtonHintsIntervalOptions,
            _repeatButtonHintsIntervalSeconds);
        int next = Math.Clamp(Math.Max(0, current) + direction, 0,
            RepeatButtonHintsIntervalOptions.Length - 1);
        if (next == current)
            return;
        SetRepeatButtonHintsIntervalFromMenu(RepeatButtonHintsIntervalOptions[next]);
        _repeatButtonHintsIntervalSlider?.SetValue(
            LocalizedRepeatIntervalValue(_repeatButtonHintsIntervalSeconds));
    }

    private void OnSpeechVoiceMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        bool oneCore = _speechMenuControlsBackendId == PrismNative.BackendIds.OneCore;
        if (!oneCore && _speechMenuControlsBackendId != PrismNative.BackendIds.Sapi)
            return;
        List<SpeechVoiceOption> voices = oneCore ? _oneCoreVoices : _sapiVoices;
        string currentId = oneCore ? _oneCoreVoiceId : _sapiVoiceId;
        if (direction == 0 || voices.Count == 0)
            return;
        int current = voices.FindIndex(voice =>
            string.Equals(voice.Id, currentId, StringComparison.OrdinalIgnoreCase));
        int next = Math.Clamp(Math.Max(0, current) + direction, 0, voices.Count - 1);
        if (next == current)
            return;
        if (oneCore)
            SetOneCoreVoiceFromMenu(voices[next].Id);
        else
            SetSapiVoiceFromMenu(voices[next].Id);
        _speechVoiceSlider?.SetValue(ReadCurrentSpeechVoiceName());
    }

    private void OnSpeechVolumeMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(CurrentSpeechVolume() + Math.Sign(movement) * 5, 5, 100);
        if (next == CurrentSpeechVolume())
            return;
        if (_speechMenuControlsBackendId == PrismNative.BackendIds.OneCore)
            SetOneCoreVolumeFromMenu(next);
        else if (_speechMenuControlsBackendId == PrismNative.BackendIds.Sapi)
            SetSapiVolumeFromMenu(next);
        else
            return;
        _speechVolumeSlider?.SetValue(CurrentSpeechVolume() + "%");
    }

    private void OnSpeechRateMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(CurrentSpeechRate() + Math.Sign(movement) * 5, 0, 100);
        if (next == CurrentSpeechRate())
            return;
        if (_speechMenuControlsBackendId == PrismNative.BackendIds.OneCore)
            SetOneCoreRateFromMenu(next);
        else if (_speechMenuControlsBackendId == PrismNative.BackendIds.Sapi)
            SetSapiRateFromMenu(next);
        else
            return;
        _speechRateSlider?.SetValue(CurrentSpeechRate().ToString());
    }

    private void OnSpeechPitchMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(CurrentSpeechPitch() + Math.Sign(movement) * 5, 0, 100);
        if (next == CurrentSpeechPitch())
            return;
        if (_speechMenuControlsBackendId == PrismNative.BackendIds.OneCore)
            SetOneCorePitchFromMenu(next);
        else if (_speechMenuControlsBackendId == PrismNative.BackendIds.Sapi)
            SetSapiPitchFromMenu(next);
        else
            return;
        _speechPitchSlider?.SetValue(CurrentSpeechPitch().ToString());
    }

    private void OnSpeechBackSubmitted()
    {
        if (!_speechMenuOpen)
            return;
        if (!_speechMenuInputReady)
        {
            WriteStatus("Ignored a BACK submit from the input that opened MOD SETTINGS.");
            return;
        }
        CancelRestoreModDefaultsConfirmation();
        CancelResetWelcomeScreenConfirmation();
        _mainMenu?.GoBack();
    }

    private void OnOpenUserGuideSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady ||
            _openUserGuideButton == null ||
            EventSystem.current?.currentSelectedGameObject?
                .GetComponentInParent<SettingsRow>()?.GetInstanceID() !=
                _openUserGuideButton.GetInstanceID())
            return;
        CancelResetWelcomeScreenConfirmation();
        CancelRestoreModDefaultsConfirmation();
        if (!OpenGuide())
            QueueSpeech(L("User's guide could not be opened."));
        else
            ResetSpeechMenuFocus();
    }

    private bool IsResetWelcomeScreenFocused()
    {
        if (_resetWelcomeScreenButton == null)
            return false;
        SettingsRow? selectedRow = EventSystem.current?
            .currentSelectedGameObject?.GetComponentInParent<SettingsRow>();
        return selectedRow != null && selectedRow.GetInstanceID() ==
            _resetWelcomeScreenButton.GetInstanceID();
    }

    private void CancelResetWelcomeScreenConfirmation()
    {
        if (_resetWelcomeScreenConfirmUntil == 0)
            return;
        _resetWelcomeScreenConfirmUntil = 0;
        _resetWelcomeScreenFirstFrame = 0;
        _resetWelcomeScreenInputReleased = false;
        _resetWelcomeScreenButton?.SetValue(string.Empty);
        WriteStatus("Reset welcome screen confirmation cancelled.");
    }

    private void OnResetWelcomeScreenSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady ||
            !IsResetWelcomeScreenFocused() ||
            Time.frameCount == _resetWelcomeScreenLastActivationFrame)
            return;
        _resetWelcomeScreenLastActivationFrame = Time.frameCount;

        long now = Environment.TickCount64;
        if (_resetWelcomeScreenConfirmUntil != 0 &&
            now < _resetWelcomeScreenConfirmUntil)
        {
            if (!_resetWelcomeScreenInputReleased ||
                Time.frameCount <= _resetWelcomeScreenFirstFrame)
                return;
            CancelResetWelcomeScreenConfirmation();
            if (ResetWelcomeScreenForNextLaunch())
                QueueSpeech(L("Welcome screen reset. It will appear the next time the game launches."));
            else
                QueueSpeech(L("Welcome screen could not be reset."));
            return;
        }

        CancelRestoreModDefaultsConfirmation();
        CancelResetWelcomeScreenConfirmation();
        _resetWelcomeScreenConfirmUntil = now + 5000;
        _resetWelcomeScreenFirstFrame = Time.frameCount;
        _resetWelcomeScreenInputReleased = false;
        _resetWelcomeScreenButton?.SetValue(L("Press again to confirm"));
        _lastSpeechMenuRowId = _resetWelcomeScreenButton!.GetInstanceID();
        _lastSpeechMenuValue = "Press again to confirm";
        QueueSpeech(L("Are you sure? Press again to confirm."));
        WriteStatus("Reset welcome screen confirmation requested.");
    }

    private bool IsRestoreModDefaultsFocused()
    {
        if (_restoreModDefaultsButton == null)
            return false;
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        SettingsRow? selectedRow = selected?.GetComponentInParent<SettingsRow>();
        return selectedRow != null &&
            selectedRow.GetInstanceID() == _restoreModDefaultsButton.GetInstanceID();
    }

    private void CancelRestoreModDefaultsConfirmation()
    {
        if (_restoreModDefaultsConfirmUntil == 0)
            return;
        _restoreModDefaultsConfirmUntil = 0;
        _restoreModDefaultsFirstFrame = 0;
        _restoreModDefaultsInputReleased = false;
        _restoreModDefaultsButton?.SetValue(string.Empty);
        WriteStatus("Restore mod defaults confirmation cancelled.");
    }

    private void OnRestoreModDefaultsSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady ||
            !IsRestoreModDefaultsFocused())
            return;
        if (Time.frameCount == _restoreModDefaultsLastActivationFrame)
            return;
        _restoreModDefaultsLastActivationFrame = Time.frameCount;

        long now = Environment.TickCount64;
        if (_restoreModDefaultsConfirmUntil != 0 &&
            now < _restoreModDefaultsConfirmUntil)
        {
            // The opening submit must be released before another activation
            // can confirm. This also guards duplicated UI submit callbacks.
            if (!_restoreModDefaultsInputReleased ||
                Time.frameCount <= _restoreModDefaultsFirstFrame)
                return;
            CancelRestoreModDefaultsConfirmation();
            RestoreModDefaults();
            return;
        }

        CancelRestoreModDefaultsConfirmation();
        _restoreModDefaultsConfirmUntil = now + 5000;
        _restoreModDefaultsFirstFrame = Time.frameCount;
        _restoreModDefaultsInputReleased = false;
        _restoreModDefaultsButton?.SetValue(L("Press again to confirm"));
        _lastSpeechMenuRowId = _restoreModDefaultsButton!.GetInstanceID();
        _lastSpeechMenuValue = "Press again to confirm";
        QueueSpeech(L("Are you sure? Press again to confirm."));
        WriteStatus("Restore mod defaults confirmation requested.");
    }

    private void RestoreModDefaults()
    {
        bool speechWasOff = !_speechEnabled;
        // These are the settings shown in Mod Settings. Native game audio,
        // the FPS limiter, and control bindings have their own ownership.
        SetOutputModeFromMenu("Auto");
        SetBrailleOutputFromMenu(true);
        SetMuteSpeechInBackgroundFromMenu(false);
        SetIndexingFromMenu(true);
        SetFilterCapitalisationFromMenu(true);
        SetReadControlTypesFromMenu(true);
        SetSliderRangesFromMenu(false);
        SetOneOnOneFeedbackFromMenu(true);
        SetHintsTypeFromMenu("Automatic");
        SetReadButtonHintsFromMenu(true);
        SetButtonHintsDelayFromMenu(10);
        SetRepeatButtonHintsFromMenu(-1);
        SetRepeatButtonHintsIntervalFromMenu(30);
        SetSapiVoiceFromMenu(string.Empty);
        SetSapiVolumeFromMenu(100);
        SetSapiRateFromMenu(50);
        SetSapiPitchFromMenu(50);
        SetOneCoreVoiceFromMenu(string.Empty);
        SetOneCoreVolumeFromMenu(100);
        SetOneCoreRateFromMenu(50);
        SetOneCorePitchFromMenu(50);
        SetSpeechEnabledFromMenu(true);
        UpdateSpeechMenuValues();
        _lastSpeechMenuValue = null;
        if (speechWasOff)
            QueueSequentialSpeech(L("Mod defaults restored."));
        else
            QueueSpeech(L("Mod defaults restored."));
        WriteStatus("Restored all Mod Settings values to defaults.");
    }

    private bool ReadSpeechMenuFocus()
    {
        if (!_speechMenuOpen || _speechMenuPanel == null || _speechMenuRoot == null)
        {
            ResetSpeechMenuFocus();
            return false;
        }
        if (_mainMenu?.panels == null || _mainMenu.panels.Count == 0 ||
            _mainMenu.panels.Peek().GetInstanceID() != _speechMenuPanel.GetInstanceID())
        {
            if (Environment.TickCount64 - _speechMenuOpenedAt < 500)
                WriteStatus("Mod Settings menu lost focus immediately after opening.");
            _speechMenuOpen = false;
            _speechMenuInputReady = false;
            CancelRestoreModDefaultsConfirmation();
            ResetSpeechMenuFocus();
            return false;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(_speechMenuRoot.transform))
        {
            if (_speechMenuInputReady &&
                Environment.TickCount64 - _speechMenuOpenedAt > 500 &&
                _speechUiOptions.Count > 0)
                EventSystem.current?.SetSelectedGameObject(_speechUiOptions[0].Row.gameObject);
            if (_speechMenuIntroductionPending &&
                Environment.TickCount64 - _speechMenuOpenedAt > 800)
            {
                _speechMenuIntroductionPending = false;
                QueueSpeech(L("Mod settings menu."));
            }
            return true;
        }

        SettingsRow? selectedRow = selected.GetComponentInParent<SettingsRow>();
        SpeechUiOption? focused = null;
        foreach (SpeechUiOption option in _speechUiOptions)
        {
            if (selectedRow != null && option.Row.GetInstanceID() == selectedRow.GetInstanceID())
            {
                focused = option;
                break;
            }
            if (option.Row.IsFocused)
                focused = option;
        }
        if (focused == null)
            return true;

        if (_restoreModDefaultsConfirmUntil != 0 &&
            focused.Row.GetInstanceID() != _restoreModDefaultsButton?.GetInstanceID())
            CancelRestoreModDefaultsConfirmation();
        if (_resetWelcomeScreenConfirmUntil != 0 &&
            focused.Row.GetInstanceID() != _resetWelcomeScreenButton?.GetInstanceID())
            CancelResetWelcomeScreenConfirmation();

        string? value = focused.ReadValue();
        int id = focused.Row.GetInstanceID();
        if (id != _lastSpeechMenuRowId)
        {
            _lastSpeechMenuRowId = id;
            _lastSpeechMenuValue = value;
            string label = WithControlType(L(focused.Label), focused.ControlType);
            string message = value == null ? label :
                LF("{0}, {1}", label, LocalizeSpeechUiValue(focused, value));
            message = WithSliderRange(message, focused.Label, focused.ControlType);
            message = WithMenuIndex(message, _speechUiOptions.IndexOf(focused),
                _speechUiOptions.Count);
            if (_speechMenuIntroductionPending)
            {
                message = L("Mod settings.") + " " + message;
                _speechMenuIntroductionPending = false;
            }
            QueueFocusSpeech(message);
        }
        else if (value != null &&
            !string.Equals(value, _lastSpeechMenuValue, StringComparison.Ordinal))
        {
            _lastSpeechMenuValue = value;
            RecordButtonHintUiActivity(Environment.TickCount64);
            if (focused.Row != _speechOutputToggle)
                QueueSpeech(LocalizeSpeechUiValue(focused, value));
        }
        return true;
    }

    private void ResetSpeechMenuFocus()
    {
        _lastSpeechMenuRowId = 0;
        _lastSpeechMenuValue = null;
    }

    private string LocalizeSpeechUiValue(SpeechUiOption option, string value) =>
        option.Label switch
        {
            "BUTTON HINTS DELAY" =>
                LocalizedButtonHintsDelayValue(_buttonHintsDelaySeconds),
            "REPEAT BUTTON HINTS" =>
                LocalizedRepeatButtonHintsValue(_repeatButtonHintsCount),
            "REPEAT INTERVAL" =>
                LocalizedRepeatIntervalValue(_repeatButtonHintsIntervalSeconds),
            "VOICE" when _speechMenuControlsBackendId == PrismNative.BackendIds.Sapi &&
                !string.IsNullOrEmpty(_sapiVoiceId) => value,
            "VOICE" when _speechMenuControlsBackendId == PrismNative.BackendIds.OneCore &&
                !string.IsNullOrEmpty(_oneCoreVoiceId) => value,
            _ => L(value)
        };

    private sealed record SpeechUiOption(string Label, string ControlType,
        SettingsRow Row,
        Func<string?> ReadValue);
}
