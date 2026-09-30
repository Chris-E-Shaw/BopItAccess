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
    private readonly List<SpeechUiOption> _speechUiOptions = new();
    private SettingsToggle? _speechOutputToggle;
    private SettingsSlider? _speechModeSlider;
    private SettingsToggle? _trimSilenceToggle;
    private SettingsSlider? _speechVoiceSlider;
    private SettingsSlider? _speechVolumeSlider;
    private SettingsSlider? _speechRateSlider;
    private SettingsSlider? _speechPitchSlider;
    private UnityAction? _speechOutputSubmitListener;
    private UnityAction? _trimSilenceSubmitListener;
    private UnityAction? _speechBackSubmitListener;
    private UnityAction<int>? _speechModeMoveListener;
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

    private void UpdateSpeechMenuUi()
    {
        if (_speechMenuOpen)
        {
            if (_speechMenuRoot == null || _speechMenuPanel == null ||
                _mainMenu == null || _mainMenu.panels == null ||
                _mainMenu.panels.Count == 0 ||
                _mainMenu.panels.Peek().GetInstanceID() != _speechMenuPanel.GetInstanceID())
            {
                if (Environment.TickCount64 - _speechMenuOpenedAt < 500)
                    WriteStatus("Speech settings menu left the panel stack immediately after opening.");
                _speechMenuOpen = false;
                _speechMenuInputReady = false;
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
                if (_speechOutputToggle != null && _speechOutputToggle.IsOn != _speechEnabled)
                    SetSpeechToggleDisplay(_speechOutputToggle, _speechEnabled);
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
                WriteStatus("SPEECH settings row could not be added: " + ex);
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
            clone.name = "BopItAccess Speech Settings";
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
            SettingsButton? button = clone.GetComponent<SettingsButton>();
            if (button == null)
                throw new InvalidOperationException("The cloned Controls row lost SettingsButton.");

            SetSpeechRowLabel(button, "SPEECH");
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
            WriteStatus("Added SPEECH menu below Controls in Settings.");
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
            ResetSpeechMenuFocus();
            WriteStatus("Opened Speech settings menu.");
        }
        catch (Exception ex)
        {
            WriteStatus("Speech settings menu could not be opened: " + ex);
            if (pushed && main.panels.Count > 0 && _speechMenuPanel != null &&
                main.panels.Peek().GetInstanceID() == _speechMenuPanel.GetInstanceID())
                main.panels.Pop();
            if (_speechMenuRoot != null)
                _speechMenuRoot.SetActive(false);
            if (settingsHidden)
                settings.Show();
            _speechMenuOpen = false;
            _speechMenuInputReady = false;
        }
    }

    private bool IsUiSubmitHeld()
    {
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

        GameObject root = new("BopItAccess Speech Menu",
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
            SetClonedLabel(title, "SPEECH");

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
            _speechModeSlider = AddSpeechSlider(settings.resolution, content,
                "OUTPUT MODE");
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
            SettingsButton back = AddSpeechButton(settings.controls, content,
                "BACK");

            _speechUiOptions.Add(new("SPEECH OUTPUT", _speechOutputToggle,
                () => _speechEnabled ? "On" : "Off"));
            _speechUiOptions.Add(new("OUTPUT MODE", _speechModeSlider,
                () => _outputMode));
            _speechUiOptions.Add(new("TRIM SILENCE", _trimSilenceToggle,
                () => _trimSilence ? "On" : "Off"));
            _speechUiOptions.Add(new("VOICE", _speechVoiceSlider,
                ReadCurrentSapiVoiceName));
            _speechUiOptions.Add(new("VOLUME", _speechVolumeSlider,
                () => _sapiVolume + "%"));
            _speechUiOptions.Add(new("RATE", _speechRateSlider,
                () => _sapiRate.ToString()));
            _speechUiOptions.Add(new("PITCH", _speechPitchSlider,
                () => _sapiPitch.ToString()));
            _speechUiOptions.Add(new("BACK", back, () => null));

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
            WriteStatus("Built Speech settings submenu with eight native-style rows.");
        }
        catch
        {
            UnityEngine.Object.Destroy(root);
            _speechUiOptions.Clear();
            _speechOutputToggle = null;
            _speechModeSlider = null;
            _trimSilenceToggle = null;
            _speechVoiceSlider = null;
            _speechVolumeSlider = null;
            _speechRateSlider = null;
            _speechPitchSlider = null;
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
            "VOICE" => _speechVoiceMoveListener ??= (UnityAction<int>)OnSpeechVoiceMoved,
            "VOLUME" => _speechVolumeMoveListener ??= (UnityAction<int>)OnSpeechVolumeMoved,
            "RATE" => _speechRateMoveListener ??= (UnityAction<int>)OnSpeechRateMoved,
            _ => _speechPitchMoveListener ??= (UnityAction<int>)OnSpeechPitchMoved
        };
        row.SliderMoved.AddListener(listener);
        clone.SetActive(true);
        return row;
    }

    private SettingsButton AddSpeechButton(SettingsButton? source, Transform parent,
        string label)
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
        _speechBackSubmitListener ??= (UnityAction)OnSpeechBackSubmitted;
        row.Submitted.AddListener(_speechBackSubmitListener);
        clone.SetActive(true);
        return row;
    }

    private static void SetSpeechRowLabel(SettingsRow row, string label)
    {
        TMP_Text? regular = FindFpsLabel(row.DefaultContainer, row.ValueText);
        TMP_Text? active = FindFpsLabel(row.ActiveContainer, row.ActiveValueText);
        if (regular == null || active == null)
            throw new InvalidOperationException("A cloned settings row has no visible labels.");
        SetClonedLabel(regular, label);
        SetClonedLabel(active, label);
        if (row.ValueText != null)
            DisableFpsValueLocalization(row.ValueText);
        if (row.ActiveValueText != null)
            DisableFpsValueLocalization(row.ActiveValueText);
    }

    private static void SetSpeechToggleDisplay(SettingsToggle row, bool enabled)
    {
        row.SetValue(enabled);
        string value = enabled ? "On" : "Off";
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

    private void UpdateSpeechMenuValues()
    {
        if (_speechOutputToggle != null)
            SetSpeechToggleDisplay(_speechOutputToggle, _speechEnabled);
        _speechModeSlider?.SetValue(_outputMode);
        if (_trimSilenceToggle != null)
            SetSpeechToggleDisplay(_trimSilenceToggle, _trimSilence);
        _speechVoiceSlider?.SetValue(ReadCurrentSapiVoiceName());
        _speechVolumeSlider?.SetValue(_sapiVolume + "%");
        _speechRateSlider?.SetValue(_sapiRate.ToString());
        _speechPitchSlider?.SetValue(_sapiPitch.ToString());
    }

    private string ReadCurrentSapiVoiceName()
    {
        foreach (SpeechVoiceOption voice in _sapiVoices)
        {
            if (string.Equals(voice.Id, _sapiVoiceId, StringComparison.OrdinalIgnoreCase))
                return voice.Name;
        }
        return _sapiVoices.Count == 0 ? "No SAPI voices installed" : _sapiVoices[0].Name;
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

    private void OnTrimSilenceSubmitted()
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
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
        _speechModeSlider?.SetValue(_outputMode);
    }

    private void OnSpeechVoiceMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int direction = Math.Sign(movement);
        if (direction == 0 || _sapiVoices.Count == 0)
            return;
        int current = _sapiVoices.FindIndex(voice =>
            string.Equals(voice.Id, _sapiVoiceId, StringComparison.OrdinalIgnoreCase));
        int next = Math.Clamp(Math.Max(0, current) + direction, 0, _sapiVoices.Count - 1);
        if (next == current)
            return;
        SetSapiVoiceFromMenu(_sapiVoices[next].Id);
        _speechVoiceSlider?.SetValue(ReadCurrentSapiVoiceName());
    }

    private void OnSpeechVolumeMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(_sapiVolume + Math.Sign(movement) * 5, 5, 100);
        if (next == _sapiVolume)
            return;
        SetSapiVolumeFromMenu(next);
        _speechVolumeSlider?.SetValue(_sapiVolume + "%");
    }

    private void OnSpeechRateMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(_sapiRate + Math.Sign(movement) * 5, 0, 100);
        if (next == _sapiRate)
            return;
        SetSapiRateFromMenu(next);
        _speechRateSlider?.SetValue(_sapiRate.ToString());
    }

    private void OnSpeechPitchMoved(int movement)
    {
        if (!_speechMenuOpen || !_speechMenuInputReady)
            return;
        int next = Math.Clamp(_sapiPitch + Math.Sign(movement) * 5, 0, 100);
        if (next == _sapiPitch)
            return;
        SetSapiPitchFromMenu(next);
        _speechPitchSlider?.SetValue(_sapiPitch.ToString());
    }

    private void OnSpeechBackSubmitted()
    {
        if (!_speechMenuOpen)
            return;
        if (!_speechMenuInputReady)
        {
            WriteStatus("Ignored a BACK submit from the input that opened SPEECH.");
            return;
        }
        _mainMenu?.GoBack();
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
                WriteStatus("Speech settings menu lost focus immediately after opening.");
            _speechMenuOpen = false;
            _speechMenuInputReady = false;
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
                QueueSpeech("Speech. Use Back to return to Settings. Voice, volume, rate, pitch, and trim silence apply to SAPI output.");
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

        string? value = focused.ReadValue();
        int id = focused.Row.GetInstanceID();
        if (id != _lastSpeechMenuRowId)
        {
            _lastSpeechMenuRowId = id;
            _lastSpeechMenuValue = value;
            string message = value == null ? focused.Label : focused.Label + ", " + value;
            if (_speechMenuIntroductionPending)
            {
                message = "Speech. Use Back to return to Settings. " +
                    "Voice, volume, rate, pitch, and trim silence apply to SAPI output. " + message;
                _speechMenuIntroductionPending = false;
            }
            QueueSpeech(message);
        }
        else if (value != null &&
            !string.Equals(value, _lastSpeechMenuValue, StringComparison.Ordinal))
        {
            _lastSpeechMenuValue = value;
            if (focused.Row != _speechOutputToggle)
                QueueSpeech(value);
        }
        return true;
    }

    private void ResetSpeechMenuFocus()
    {
        _lastSpeechMenuRowId = 0;
        _lastSpeechMenuValue = null;
    }

    private sealed record SpeechUiOption(string Label, SettingsRow Row,
        Func<string?> ReadValue);
}
