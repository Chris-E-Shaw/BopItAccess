using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

// Extend the game's Controls panel with the four leaderboard axis parts.
// The added rows use their own interactive rebind operation; patching the
// game's global native binding selector caused an access violation in 0.5.4.
public sealed partial class BopItAccessMod
{
    private const string LeaderboardControlsMap = "Leaderboard";
    private const string GroupAction = "ChangeGroup";
    private const string DateAction = "ChangeDateRange";
    private readonly Dictionary<int, AddedLeaderboardControlRow> _leaderboardControlRows = new();
    private readonly List<GameObject> _leaderboardAddedRows = new();
    private readonly List<InputActionReference> _leaderboardPromptReferences = new();
    private GameObject? _leaderboardControlsViewport;
    private ScrollRect? _leaderboardControlsScroll;
    private ContentSizeFitter? _leaderboardControlsFitter;
    private RectTransform? _leaderboardControlsContent;
    private RectTransform? _leaderboardControlsOriginalParent;
    private Vector2 _leaderboardContentOriginalAnchorMin;
    private Vector2 _leaderboardContentOriginalAnchorMax;
    private Vector2 _leaderboardContentOriginalPivot;
    private Vector2 _leaderboardContentOriginalPosition;
    private Vector2 _leaderboardContentOriginalSize;
    private int _leaderboardControlsPanelId;
    private long _nextLeaderboardControlsProbeAt;
    private bool _leaderboardControlsAttempted;
    private string? _customControlRowsLocale;

    public override void OnUpdate()
    {
        UpdateGameLocale();
        RefreshLocalizedControlRows();
        UpdateWelcomeScreen();
        UpdateGuideUi();
        UpdateGuideMusicFilter();
        UpdateFirstRunNativeAudioDefaults();
        UpdateBackgroundAudio();
        UpdateFpsLimitSetting();
        InitializeSpeechToggleOnMainThread();
        UpdateBackgroundSpeechFocus();
        UpdateSpeechMenuUi();
        UpdateControlsSubmitGate();
        UpdateDescriptionControlRebinding();
        UpdateResetGyroControlRebinding();
        UpdateScoreControlRebinding();
        UpdateSpeakHintsControlRebinding();
        UpdateToggleSpeechControlRebinding();
        UpdateChangeSpeechOutputControlRebinding();
        UpdateLeaderboardControlRebinding();
        UpdateNativeBindingConflictGuard();
        UpdateSpeechToggleFromInput();
        UpdateChangeSpeechOutputFromInput();
        UpdateHintInputDevice();
        UpdateOneOnOneFeedback();
        UpdateSpeakHintsOnDemand();
        if (_leaderboardControlsScroll != null)
            ScrollSelectedControlIntoView();

        long now = Environment.TickCount64;
        if (now < _nextLeaderboardControlsProbeAt)
            return;

        _nextLeaderboardControlsProbeAt = now + 250;
        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();

        Panel? panel = main?.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            // A panel can become visible before its localized labels or input
            // references finish initializing. Retry a failed injection when it
            // is opened again, while keeping successfully added rows intact.
            if (_leaderboardAddedRows.Count == 0)
                _leaderboardControlsAttempted = false;
            return;
        }

        int panelId = panel.GetInstanceID();
        if (panelId != _leaderboardControlsPanelId)
        {
            if (_leaderboardControlsPanelId != 0)
                RemoveAddedLeaderboardControls();
            _leaderboardControlsPanelId = panelId;
            _leaderboardControlsAttempted = false;
        }

        if (_leaderboardControlsAttempted)
            return;

        _leaderboardControlsAttempted = true;
        try
        {
            if (TryAddLeaderboardControls(panel, out string reason))
                WriteStatus("Added Reset Gyro and the mod binding rows to Controls.");
            else
                WriteStatus("Leaderboard binding rows were not added: " + reason);
        }
        catch (Exception ex)
        {
            RemoveAddedLeaderboardControls();
            WriteStatus("Leaderboard binding rows were not added: " + ex);
        }
    }

    private bool TryAddLeaderboardControls(Panel panel, out string reason)
    {
        InputRebindingManager? manager = panel.GetComponentInChildren<InputRebindingManager>(true);
        if (manager == null)
        {
            reason = "the rebinding manager is unavailable";
            return false;
        }

        ControlRow? template = null;
        foreach (ControlRow row in panel.GetComponentsInChildren<ControlRow>(true))
        {
            if (row.ActionName == "Bop")
            {
                template = row;
                break;
            }
        }

        ResetToDefaultRow? reset = panel.GetComponentInChildren<ResetToDefaultRow>(true);
        if (template == null || reset == null ||
            template.transform.parent != reset.transform.parent)
        {
            reason = "the native binding rows and Reset do not share a container";
            return false;
        }

        Transform parent = template.transform.parent;
        if (parent.GetComponent<LayoutGroup>() == null)
        {
            reason = "the binding container has no layout group";
            return false;
        }

        Selectable? templateSelectable = template.GetComponentInChildren<Selectable>(true);
        Selectable? resetSelectable = reset.GetComponentInChildren<Selectable>(true);
        if (templateSelectable == null || resetSelectable == null ||
            templateSelectable.navigation.mode != Navigation.Mode.Vertical ||
            resetSelectable.navigation.mode != Navigation.Mode.Vertical)
        {
            reason = "the native rows do not use vertical UI navigation";
            return false;
        }

        if (FindControlLabel(template.transform, "Default/Label") == null ||
            FindControlLabel(template.transform, "Active/Label") == null)
        {
            reason = "the Bop row's two visible labels could not be found";
            return false;
        }

        RectTransform? contentRect = parent.GetComponent<RectTransform>();
        RectTransform? tableRect = parent.parent?.GetComponent<RectTransform>();
        if (contentRect == null || tableRect == null ||
            tableRect.GetComponent<ScrollRect>() != null)
        {
            reason = "the expected Controls table geometry is unavailable";
            return false;
        }

        InputActionAsset? asset = manager.inputActions ?? manager.playerInput?.actions;
        InputAction? group = asset?.FindActionMap(LeaderboardControlsMap, false)?
            .FindAction(GroupAction, false);
        InputAction? date = asset?.FindActionMap(LeaderboardControlsMap, false)?
            .FindAction(DateAction, false);
        if (!HasBothDeviceComposites(group) || !HasBothDeviceComposites(date))
        {
            reason = "the expected keyboard and gamepad 1D axes are missing";
            return false;
        }

        LeaderboardControlPart[] parts =
        {
            new(GroupAction, "negative", "Leaderboard group previous", "GROUP PREVIOUS"),
            new(GroupAction, "positive", "Leaderboard group next", "GROUP NEXT"),
            new(DateAction, "negative", "Leaderboard date previous", "DATE PREVIOUS"),
            new(DateAction, "positive", "Leaderboard date next", "DATE NEXT")
        };

        try
        {
            int insertAt = reset.transform.GetSiblingIndex();
            AddResetGyroControlRow(template, parent, insertAt++, manager);
            foreach (LeaderboardControlPart part in parts)
            {
                InputAction action = part.Action == GroupAction ? group! : date!;
                GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
                clone.SetActive(false);
                _leaderboardAddedRows.Add(clone);
                clone.name = part.Label.Replace(' ', '_');
                clone.transform.SetSiblingIndex(insertAt++);

                ControlRow? row = clone.GetComponent<ControlRow>();
                TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
                TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
                Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
                if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
                    throw new InvalidOperationException("A cloned row lost its native controls.");

                row.ActionMapName = LeaderboardControlsMap;
                row.ActionName = part.Action;
                SetClonedLabel(defaultLabel, part.VisibleLabel);
                SetClonedLabel(activeLabel, part.VisibleLabel);

                InputActionReference actionReference = InputActionReference.Create(action);
                _leaderboardPromptReferences.Add(actionReference);
                ControlPromptSpriteSwapper.CompositePart composite =
                    part.PartName == "negative"
                        ? ControlPromptSpriteSwapper.CompositePart.Negative
                        : ControlPromptSpriteSwapper.CompositePart.Positive;
                SetDisplayPrompt(row.ActiveDisplayPrompt, actionReference, composite);
                SetDisplayPrompt(row.DefaultDisplayPrompt, actionReference, composite);

                // The selectable is a separate component. Remove the cloned
                // ControlRow before activation so its native OnSubmit and
                // rebinding listeners cannot start a second operation.
                Navigation navigation = selectable.navigation;
                navigation.mode = Navigation.Mode.Vertical;
                selectable.navigation = navigation;
                GameObject? active = row.ActiveContainer;
                GameObject? inactive = row.DefaultContainer;
                if (active == null || inactive == null)
                    throw new InvalidOperationException("A cloned row lost its focus containers.");
                _leaderboardControlRows[clone.GetInstanceID()] = new(
                    clone, part, active, inactive,
                    row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
                row.enabled = false;
                UnityEngine.Object.Destroy(row);
            }

            AddDescriptionControlRow(template, parent, insertAt++);
            AddScoreControlRow(template, parent, insertAt++);
            AddToggleSpeechControlRow(template, parent, insertAt++);
            AddSpeakHintsControlRow(template, parent, insertAt++);
            AddChangeSpeechOutputControlRow(template, parent, insertAt);

            AddLeaderboardControlsViewport(tableRect, contentRect);
            foreach (GameObject row in _leaderboardAddedRows)
                row.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            if (_leaderboardControlsScroll != null)
                _leaderboardControlsScroll.verticalNormalizedPosition = 1f;
            // ReadControlsFocus already has the native rows. The added rows
            // are tracked separately, so keep this visit's introduction state.
            reason = string.Empty;
            return true;
        }
        catch
        {
            RemoveAddedLeaderboardControls();
            throw;
        }
    }

    private static TMP_Text? FindControlLabel(Transform row, string path) =>
        row.Find(path)?.GetComponent<TMP_Text>();

    private static void SetClonedLabel(TMP_Text label, string text)
    {
        // These two scene labels each have a LocalizeStringEvent that would
        // otherwise restore the original Bop text when the clone is enabled.
        Component? localization = label.GetComponent("LocalizeStringEvent");
        Behaviour? localizationBehaviour = localization?.TryCast<Behaviour>();
        if (localizationBehaviour != null)
            localizationBehaviour.enabled = false;

        label.text = L(text);
    }

    private void RefreshLocalizedControlRows()
    {
        string locale = CurrentGameLocale;
        if (string.Equals(_customControlRowsLocale, locale,
            StringComparison.Ordinal))
            return;
        _customControlRowsLocale = locale;
        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
            RefreshLocalizedControlRow(row.Root, row.Part.VisibleLabel);
        RefreshLocalizedControlRow(_resetGyroControlRow?.Root, "RESET GYRO");
        RefreshLocalizedControlRow(_descriptionControlRow?.Root,
            "READ DESCRIPTIONS");
        RefreshLocalizedControlRow(_scoreControlRow?.Root, "READ SCORE");
        RefreshLocalizedControlRow(_toggleSpeechControlRow?.Root,
            "TOGGLE SPEECH");
        RefreshLocalizedControlRow(_speakHintsControlRow?.Root,
            "SPEAK HINTS");
        RefreshLocalizedControlRow(_changeSpeechOutputControlRow?.Root,
            "CHANGE SPEECH OUTPUT");
    }

    private static void RefreshLocalizedControlRow(GameObject? root,
        string label)
    {
        if (root == null)
            return;
        TMP_Text? regular = FindControlLabel(root.transform, "Default/Label");
        TMP_Text? focused = FindControlLabel(root.transform, "Active/Label");
        if (regular != null)
            SetClonedLabel(regular, label);
        if (focused != null)
            SetClonedLabel(focused, label);
    }

    private static void SetDisplayPrompt(ControlPromptSpriteSwapperV2? prompt,
        InputActionReference reference,
        ControlPromptSpriteSwapper.CompositePart part)
    {
        if (prompt == null)
            return;

        prompt.ActionReference = reference;
        prompt.Composite = part;
    }

    private void AddLeaderboardControlsViewport(RectTransform table,
        RectTransform content)
    {
        _leaderboardControlsOriginalParent = table;
        _leaderboardControlsContent = content;
        _leaderboardContentOriginalAnchorMin = content.anchorMin;
        _leaderboardContentOriginalAnchorMax = content.anchorMax;
        _leaderboardContentOriginalPivot = content.pivot;
        _leaderboardContentOriginalPosition = content.anchoredPosition;
        _leaderboardContentOriginalSize = content.sizeDelta;

        int originalSibling = content.GetSiblingIndex();
        GameObject viewport = new("BopItAccess Controls Viewport",
            new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
        _leaderboardControlsViewport = viewport;
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.SetParent(table, false);
        viewportRect.SetSiblingIndex(originalSibling);
        viewportRect.anchorMin = _leaderboardContentOriginalAnchorMin;
        viewportRect.anchorMax = _leaderboardContentOriginalAnchorMax;
        viewportRect.pivot = _leaderboardContentOriginalPivot;
        viewportRect.anchoredPosition = _leaderboardContentOriginalPosition;
        viewportRect.sizeDelta = _leaderboardContentOriginalSize;
        viewport.AddComponent<RectMask2D>();

        content.SetParent(viewportRect, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        _leaderboardControlsFitter = content.gameObject.AddComponent<ContentSizeFitter>();
        _leaderboardControlsFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        _leaderboardControlsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = table.gameObject.AddComponent<ScrollRect>();
        _leaderboardControlsScroll = scroll;
        scroll.viewport = viewportRect;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.inertia = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
    }

    private void ScrollSelectedControlIntoView()
    {
        ScrollRect? scroll = _leaderboardControlsScroll;
        if (scroll == null || scroll.viewport == null || scroll.content == null)
            return;

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(scroll.content))
            return;

        AddedLeaderboardControlRow? added = FindAddedLeaderboardControlRow(selected);
        AddedResetGyroControlRow? resetGyro = FindAddedResetGyroControlRow(selected);
        AddedDescriptionControlRow? description = FindAddedDescriptionControlRow(selected);
        AddedScoreControlRow? score = FindAddedScoreControlRow(selected);
        AddedToggleSpeechControlRow? toggleSpeech =
            FindAddedToggleSpeechControlRow(selected);
        AddedSpeakHintsControlRow? speakHints =
            FindAddedSpeakHintsControlRow(selected);
        AddedChangeSpeechOutputControlRow? changeOutput =
            FindAddedChangeSpeechOutputControlRow(selected);
        ControlRow? control = selected.GetComponentInParent<ControlRow>();
        ResetToDefaultRow? reset = selected.GetComponentInParent<ResetToDefaultRow>();
        Transform item = added != null ? added.Root.transform :
            resetGyro != null ? resetGyro.Root.transform :
            description != null ? description.Root.transform :
            score != null ? score.Root.transform :
            toggleSpeech != null ? toggleSpeech.Root.transform :
            speakHints != null ? speakHints.Root.transform :
            changeOutput != null ? changeOutput.Root.transform :
            control != null ? control.transform :
            reset != null ? reset.transform : selected.transform;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            scroll.viewport, item);
        Rect viewport = scroll.viewport.rect;
        float overflow = bounds.min.y < viewport.yMin
            ? bounds.min.y - viewport.yMin
            : bounds.max.y > viewport.yMax
                ? bounds.max.y - viewport.yMax
                : 0f;
        if (Mathf.Abs(overflow) < 0.5f)
            return;

        float scrollableHeight = scroll.content.rect.height - viewport.height;
        if (scrollableHeight > 0f)
            scroll.verticalNormalizedPosition = Mathf.Clamp01(
                scroll.verticalNormalizedPosition + overflow / scrollableHeight);
    }

    private static bool HasBothDeviceComposites(InputAction? action)
    {
        if (action == null)
            return false;

        bool keyboardNegative = false;
        bool keyboardPositive = false;
        bool gamepadNegative = false;
        bool gamepadPositive = false;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite)
                continue;

            string path = binding.path ?? string.Empty;
            bool negative = string.Equals(binding.name, "negative", StringComparison.OrdinalIgnoreCase);
            bool positive = string.Equals(binding.name, "positive", StringComparison.OrdinalIgnoreCase);
            if (path.Contains("<Keyboard>", StringComparison.OrdinalIgnoreCase))
            {
                keyboardNegative |= negative;
                keyboardPositive |= positive;
            }
            else if (path.Contains("<Gamepad>", StringComparison.OrdinalIgnoreCase))
            {
                gamepadNegative |= negative;
                gamepadPositive |= positive;
            }
        }

        return keyboardNegative && keyboardPositive && gamepadNegative && gamepadPositive;
    }

    private static int FindCompositePartIndex(InputAction action, string deviceType,
        int originalIndex, string partName)
    {
        deviceType ??= string.Empty;
        bool gamepad = deviceType.Contains("Gamepad", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("Controller", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("Xbox", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("XInput", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("PlayStation", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("Dual", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("Switch", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("NPad", StringComparison.OrdinalIgnoreCase);
        bool keyboard = deviceType.Contains("Keyboard", StringComparison.OrdinalIgnoreCase) ||
            deviceType.Contains("Standalone", StringComparison.OrdinalIgnoreCase);
        string? layout = gamepad ? "<Gamepad>" : keyboard ? "<Keyboard>" : null;
        // Prefer the composite identified by the game's original device
        // selection. This preserves its controller-family matching logic.
        if (originalIndex >= 0 && originalIndex < action.bindings.Count)
        {
            int root = originalIndex;
            while (root >= 0 && action.bindings[root].isPartOfComposite)
                root--;

            if (root >= 0 && action.bindings[root].isComposite)
            {
                int part = FindPartInComposite(action, root, partName);
                if (part >= 0 && (layout == null ||
                    (action.bindings[part].path ?? string.Empty)
                        .Contains(layout, StringComparison.OrdinalIgnoreCase)))
                    return part;
            }
        }

        // Some versions of the native selector return -1 for composite roots.
        layout ??= "<Keyboard>";
        for (int root = 0; root < action.bindings.Count; root++)
        {
            if (!action.bindings[root].isComposite)
                continue;

            int part = FindPartInComposite(action, root, partName);
            if (part >= 0 &&
                (action.bindings[part].path ?? string.Empty)
                    .Contains(layout, StringComparison.OrdinalIgnoreCase))
                return part;
        }

        return -1;
    }

    private static int FindPartInComposite(InputAction action, int root, string partName)
    {
        for (int i = root + 1; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite)
                break;

            if (string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private void RemoveAddedLeaderboardControls()
    {
        if (_resetGyroRebindOperation != null)
            RestoreResetGyroOriginalOverride();
        if (_changeSpeechOutputRebindOperation != null)
            RestoreChangeSpeechOutputOriginalOverride();
        CancelDescriptionControlRebinding(false);
        CancelResetGyroControlRebinding(false);
        CancelScoreControlRebinding(false);
        CancelToggleSpeechControlRebinding(false);
        CancelSpeakHintsControlRebinding(false);
        CancelChangeSpeechOutputControlRebinding(false);
        CancelLeaderboardControlRebinding(false);
        if (_leaderboardControlsContent != null &&
            _leaderboardControlsOriginalParent != null)
        {
            RectTransform content = _leaderboardControlsContent;
            content.SetParent(_leaderboardControlsOriginalParent, false);
            content.anchorMin = _leaderboardContentOriginalAnchorMin;
            content.anchorMax = _leaderboardContentOriginalAnchorMax;
            content.pivot = _leaderboardContentOriginalPivot;
            content.anchoredPosition = _leaderboardContentOriginalPosition;
            content.sizeDelta = _leaderboardContentOriginalSize;
        }

        if (_leaderboardControlsScroll != null)
            UnityEngine.Object.Destroy(_leaderboardControlsScroll);
        if (_leaderboardControlsFitter != null)
            UnityEngine.Object.Destroy(_leaderboardControlsFitter);
        if (_leaderboardControlsViewport != null)
            UnityEngine.Object.Destroy(_leaderboardControlsViewport);

        foreach (GameObject row in _leaderboardAddedRows)
        {
            if (row != null)
                UnityEngine.Object.Destroy(row);
        }
        foreach (InputActionReference reference in _leaderboardPromptReferences)
        {
            if (reference != null)
                UnityEngine.Object.Destroy(reference);
        }

        _leaderboardAddedRows.Clear();
        _leaderboardPromptReferences.Clear();
        _leaderboardControlRows.Clear();
        _descriptionControlRow = null;
        _resetGyroControlRow = null;
        _scoreControlRow = null;
        _toggleSpeechControlRow = null;
        _speakHintsControlRow = null;
        _changeSpeechOutputControlRow = null;
        _leaderboardControlsScroll = null;
        _leaderboardControlsFitter = null;
        _leaderboardControlsViewport = null;
        _leaderboardControlsContent = null;
        _leaderboardControlsOriginalParent = null;
    }

    private sealed record LeaderboardControlPart(
        string Action, string PartName, string Label, string VisibleLabel);

    private sealed record AddedLeaderboardControlRow(
        GameObject Root, LeaderboardControlPart Part, GameObject ActiveContainer,
        GameObject DefaultContainer, ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);
}
