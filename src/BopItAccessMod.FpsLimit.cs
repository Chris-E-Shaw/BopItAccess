using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string FpsLimitPreference = "BopItAccess.LimitFps";
    private static readonly int[] FpsLimitChoices = { 30, 60, 120, 240, -1 };
    private int _fpsLimit = 60;
    private bool _fpsLimitInitialized;
    private SettingsSlider? _fpsSettingsSlider;
    private UnityAction<int>? _fpsSliderMovedListener;
    private int _fpsSettingsPanelId;
    private string? _fpsSettingsRenderedLocale;
    private long _nextFpsSettingsProbeAt;
    private long _nextFpsSettingsErrorLogAt;

    private void UpdateFpsLimitSetting()
    {
        if (!_fpsLimitInitialized)
        {
            int saved = PlayerPrefs.GetInt(FpsLimitPreference, 60);
            _fpsLimit = Array.IndexOf(FpsLimitChoices, saved) >= 0 ? saved : 60;
            _fpsLimitInitialized = true;
            WriteStatus($"FPS limit: {FpsLimitText(_fpsLimit)}.");
        }

        // Unity's desktop targetFrameRate is ignored while vSync is enabled.
        // Do not change Time.timeScale or fixedDeltaTime: this setting is only
        // a rendering/frame-loop cap, never a game speed multiplier.
        if (QualitySettings.vSyncCount != 0)
            QualitySettings.vSyncCount = 0;
        if (Application.targetFrameRate != _fpsLimit)
            Application.targetFrameRate = _fpsLimit;

        long now = Environment.TickCount64;
        if (now < _nextFpsSettingsProbeAt)
            return;
        _nextFpsSettingsProbeAt = now + 250;

        SettingsPanel? panel = _settingsPanel;
        if (panel == null)
            panel = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
            return;

        int panelId = panel.GetInstanceID();
        if (_fpsSettingsSlider != null && _fpsSettingsPanelId == panelId)
        {
            if (!string.Equals(_fpsSettingsRenderedLocale,
                CurrentGameLocale, StringComparison.Ordinal))
            {
                _fpsSettingsRenderedLocale = CurrentGameLocale;
                RefreshFpsLocalizedRow(_fpsSettingsSlider);
            }
            return;
        }

        try
        {
            AddFpsSettingsSlider(panel);
        }
        catch (Exception ex)
        {
            if (now >= _nextFpsSettingsErrorLogAt)
            {
                WriteStatus($"LIMIT FPS row could not be added: {ex}");
                MelonLoader.MelonLogger.Warning($"LIMIT FPS row could not be added: {ex.Message}");
                _nextFpsSettingsErrorLogAt = now + 5000;
            }
        }
    }

    private void AddFpsSettingsSlider(SettingsPanel panel)
    {
        SettingsSlider? template = panel.resolution;
        if (template == null)
            throw new InvalidOperationException("Resolution slider is unavailable.");

        Transform parent = template.transform.parent;
        if (parent == null)
            throw new InvalidOperationException("Resolution slider has no parent.");

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        try
        {
            clone.name = "LimitFps";
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
            SettingsSlider? slider = clone.GetComponent<SettingsSlider>();
            if (slider == null)
                throw new InvalidOperationException("Cloned Resolution row has no SettingsSlider.");

            TMP_Text? defaultLabel = FindFpsLabel(slider.DefaultContainer, slider.ValueText);
            TMP_Text? activeLabel = FindFpsLabel(slider.ActiveContainer, slider.ActiveValueText);
            if (defaultLabel == null || activeLabel == null)
                throw new InvalidOperationException("Cloned slider labels are unavailable.");
            SetClonedLabel(defaultLabel, L("LIMIT FPS"));
            SetClonedLabel(activeLabel, L("LIMIT FPS"));
            if (slider.ValueText != null)
                DisableFpsValueLocalization(slider.ValueText);
            if (slider.ActiveValueText != null)
                DisableFpsValueLocalization(slider.ActiveValueText);

            // A cloned UnityEvent retains serialized listeners from Resolution.
            // Replacing the event prevents left/right from changing resolution.
            slider.SliderMoved = new UnityEvent<int>();
            slider.SetValue(L(FpsLimitText(_fpsLimit)));
            _fpsSliderMovedListener ??= (UnityAction<int>)OnFpsSliderMoved;
            slider.SliderMoved.AddListener(_fpsSliderMovedListener);

            _fpsSettingsSlider = slider;
            _fpsSettingsPanelId = panel.GetInstanceID();
            _fpsSettingsRenderedLocale = CurrentGameLocale;
            clone.SetActive(true);
            RectTransform? content = parent.GetComponent<RectTransform>();
            if (content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (_settingsWasVisible)
                _settingsOptions = CreateSettingsOptions(panel);
            WriteStatus("Added LIMIT FPS slider to Settings.");
        }
        catch
        {
            _fpsSettingsSlider = null;
            _fpsSettingsPanelId = 0;
            UnityEngine.Object.Destroy(clone);
            throw;
        }
    }

    private void RefreshFpsLocalizedRow(SettingsSlider slider)
    {
        TMP_Text? regular = FindFpsLabel(slider.DefaultContainer,
            slider.ValueText);
        TMP_Text? focused = FindFpsLabel(slider.ActiveContainer,
            slider.ActiveValueText);
        if (regular != null)
            SetClonedLabel(regular, L("LIMIT FPS"));
        if (focused != null)
            SetClonedLabel(focused, L("LIMIT FPS"));
        slider.SetValue(L(FpsLimitText(_fpsLimit)));
    }

    private static TMP_Text? FindFpsLabel(GameObject? container, TMP_Text? value)
    {
        if (container == null)
            return null;
        TMP_Text? fallback = null;
        foreach (TMP_Text text in container.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null || text == value)
                continue;
            if (string.Equals(text.name, "Label", StringComparison.OrdinalIgnoreCase))
                return text;
            fallback ??= text;
        }
        return fallback;
    }

    private static void DisableFpsValueLocalization(TMP_Text value)
    {
        // Resolution's value objects are localized independently of its
        // labels. Keep a language refresh from restoring a resolution string.
        Component? localization = value.GetComponent("LocalizeStringEvent");
        Behaviour? behaviour = localization?.TryCast<Behaviour>();
        if (behaviour != null)
            behaviour.enabled = false;
    }

    private void OnFpsSliderMoved(int movement)
    {
        int direction = Math.Sign(movement);
        if (direction == 0)
            return;
        int current = Array.IndexOf(FpsLimitChoices, _fpsLimit);
        int next = Math.Clamp(current + direction, 0, FpsLimitChoices.Length - 1);
        if (next == current)
            return;

        _fpsLimit = FpsLimitChoices[next];
        PlayerPrefs.SetInt(FpsLimitPreference, _fpsLimit);
        SaveModPreferencesAndConfig();
        _fpsSettingsSlider?.SetValue(L(FpsLimitText(_fpsLimit)));
        // Apply on the same frame so the displayed value matches the cap.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _fpsLimit;
        WriteStatus($"FPS limit changed to {FpsLimitText(_fpsLimit)}.");
    }

    private static string FpsLimitText(int limit) =>
        limit < 0 ? "UNLIMITED" : limit.ToString();
}
