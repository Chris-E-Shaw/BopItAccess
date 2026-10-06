using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
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
    private const int MaximumGuideBytes = 4 * 1024 * 1024;
    private const int MaximumGuideNodes = 50000;
    private const int MaximumGuideDepth = 128;
    private const int MaximumGuideLines = 20000;
    private const string GuideFileName = "BopItAccess-user-guide.html";
    private readonly List<GuideTopic> _guideTopics = new();
    private readonly List<SettingsButton> _guideTopicRows = new();
    private UnityAction? _guideTopicAction;
    private UnityAction? _guideRepeatAction;
    private GameObject? _guideTopicsRoot;
    private GameObject? _guidePageRoot;
    private Panel? _guideTopicsPanel;
    private Panel? _guidePagePanel;
    private ScrollRect? _guideTopicsScroll;
    private ScrollRect? _guidePageScroll;
    private TMP_Text? _guidePageText;
    private TMP_Text? _guidePageTitle;
    private SettingsButton? _guideRepeatRow;
    private bool _guideOpen;
    private bool _guidePageOpen;
    private bool _guideInputReady;
    private int _guideOpenedFrame;
    private int _guideTopicIndex;
    private int _guideLineIndex;
    private int _guideColumnIndex;
    private int _guideLastLineDirection = 1;
    private int _guideLastTopicRowId;
    private string? _guideLastPageFocus;
    private int _guideMoveDirection;
    private long _guideNextMoveAt;
    private long _nextGuideErrorAt;

    // Called by the Welcome and Mod Settings rows. The source panel stays on
    // MainMenuUIManager's stack, so the game's Back action restores it.
    private bool OpenGuide()
    {
        if (_guideOpen)
            return true;
        UpdateGameLocale(force: true);
        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        Panel? source = main?.panels != null && main.panels.Count > 0
            ? main.panels.Peek() : null;
        SettingsPanel? settings = _settingsPanel;
        if (settings == null)
            settings = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
        if (main == null || source == null || settings?.controls == null)
        {
            QueueSpeech(L("The user's guide is unavailable because the game menu is not ready."));
            return false;
        }
        _mainMenu = main;

        string gameDirectory = Path.GetDirectoryName(Environment.ProcessPath ??
            string.Empty) ?? Environment.CurrentDirectory;
        string documentation = Path.Combine(gameDirectory, "documentation");
        string englishGuidePath = Path.Combine(documentation, GuideFileName);
        string guidePath = englishGuidePath;
        if (CurrentGameLocale != "en")
        {
            string localizedPath = Path.Combine(documentation,
                CurrentGameLocale, GuideFileName);
            if (File.Exists(localizedPath))
                guidePath = localizedPath;
            else
                WriteStatus("Guide for " + CurrentGameLocale +
                    " is missing; using the English document.");
        }
        List<GuideTopic> topics;
        try
        {
            topics = ReadGuideDocument(guidePath);
        }
        catch (Exception ex)
        {
            if (!string.Equals(guidePath, englishGuidePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                WriteStatus("Could not load the guide for " + CurrentGameLocale +
                    " from " + guidePath + ": " + ex +
                    ". Trying the English document.");
                guidePath = englishGuidePath;
                try
                {
                    topics = ReadGuideDocument(guidePath);
                }
                catch (Exception fallbackError)
                {
                    WriteStatus("Could not load the English user's guide from " +
                        guidePath + ": " + fallbackError);
                    QueueSpeech(L("The user's guide could not be opened. Check the documentation folder in the game directory."));
                    return false;
                }
            }
            else
            {
                WriteStatus("Could not load the user's guide from " + guidePath +
                    ": " + ex);
                QueueSpeech(L("The user's guide could not be opened. Check the documentation folder in the game directory."));
                return false;
            }
        }
        if (topics.Count == 0)
        {
            QueueSpeech(L("The user's guide has no topics in its table of contents."));
            return false;
        }

        bool sourceHidden = false;
        bool pushed = false;
        try
        {
            DestroyGuidePanels();
            _guideTopics.AddRange(topics);
            BuildGuidePanels(main, settings);
            if (_guideTopicsPanel == null || _guideTopicsRoot == null ||
                _guideTopicRows.Count == 0)
                throw new InvalidOperationException("The guide topic panel is incomplete.");

            source.lastSelectedButton = EventSystem.current?.currentSelectedGameObject;
            source.Hide();
            sourceHidden = true;
            _guideTopicsRoot.SetActive(true);
            _guideTopicsPanel.lastSelectedButton = null;
            _guideTopicsPanel.firstSelectedButton = _guideTopicRows[0].gameObject;
            _guideTopicsPanel.Show();
            main.panels.Push(_guideTopicsPanel);
            pushed = true;
            _guideOpen = true;
            _guidePageOpen = false;
            _guideTopicIndex = 0;
            _guideLastTopicRowId = 0;
            _guideLastPageFocus = null;
            GuideGateOpeningInput();
            SetGuideMusicFilter(true);
            WriteStatus("Opened user's guide from " + guidePath + " with " +
                _guideTopics.Count + " topics.");
            return true;
        }
        catch (Exception ex)
        {
            WriteStatus("Could not build the user's guide menu: " + ex);
            if (pushed && main.panels.Count > 0 && _guideTopicsPanel != null &&
                main.panels.Peek().GetInstanceID() == _guideTopicsPanel.GetInstanceID())
                main.panels.Pop();
            DestroyGuidePanels();
            _guideOpen = false;
            _guidePageOpen = false;
            if (sourceHidden)
                source.Show();
            SetGuideMusicFilter(false);
            QueueSpeech(L("The user's guide could not be displayed."));
            return false;
        }
    }

    private void BuildGuidePanels(MainMenuUIManager main, SettingsPanel settings)
    {
        (GameObject topicsRoot, Panel topicsPanel, Transform topicContent,
            TMP_Text topicTitle, ScrollRect? topicScroll) =
            CreateGuidePanelShell(main, "BopItAccess Guide Topics");
        _guideTopicsRoot = topicsRoot;
        _guideTopicsPanel = topicsPanel;
        _guideTopicsScroll = topicScroll;
        SetClonedLabel(topicTitle, L("USER'S GUIDE: TOPICS"));

        _guideTopicAction ??= (UnityAction)OnGuideTopicSubmitted;
        for (int index = 0; index < _guideTopics.Count; index++)
        {
            SettingsButton row = AddSpeechButton(settings.controls, topicContent,
                _guideTopics[index].Title, _guideTopicAction);
            _guideTopicRows.Add(row);
        }
        topicsPanel.firstSelectedButton = _guideTopicRows[0].gameObject;
        topicsPanel.lastSelectedButton = null;
        if (topicContent is RectTransform topicRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(topicRect);

        (GameObject pageRoot, Panel pagePanel, Transform pageContent,
            TMP_Text pageTitle, ScrollRect? pageScroll) =
            CreateGuidePanelShell(main, "BopItAccess Guide Page");
        _guidePageRoot = pageRoot;
        _guidePagePanel = pagePanel;
        _guidePageScroll = pageScroll;
        _guidePageTitle = pageTitle;

        GameObject body = UnityEngine.Object.Instantiate(pageTitle.gameObject,
            pageContent, false);
        body.name = "BopItAccess Guide Current Line";
        _guidePageText = body.GetComponent<TMP_Text>() ??
            throw new InvalidOperationException("The guide line text was unavailable.");
        SetClonedLabel(_guidePageText, string.Empty);
        _guidePageText.enableWordWrapping = true;
        _guidePageText.enableAutoSizing = false;
        _guidePageText.fontSize = Mathf.Max(22f, pageTitle.fontSize * 0.75f);
        ContentSizeFitter? textFitter = body.GetComponent<ContentSizeFitter>();
        if (textFitter == null)
            textFitter = body.AddComponent<ContentSizeFitter>();
        textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        LayoutElement? textLayout = body.GetComponent<LayoutElement>();
        if (textLayout == null)
            textLayout = body.AddComponent<LayoutElement>();
        textLayout.minHeight = 96f;

        _guideRepeatAction ??= (UnityAction)RepeatGuideLine;
        _guideRepeatRow = AddSpeechButton(settings.controls, pageContent,
            L("READ CURRENT LINE"), _guideRepeatAction);
        pagePanel.firstSelectedButton = _guideRepeatRow.gameObject;
        pagePanel.lastSelectedButton = null;
        if (pageContent is RectTransform pageRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
    }

    private (GameObject Root, Panel Panel, Transform Content,
        TMP_Text Title, ScrollRect? Scroll) CreateGuidePanelShell(
        MainMenuUIManager main, string name)
    {
        Panel? controls = main.controlsPanel;
        RectTransform? sourceRect = controls?.GetComponent<RectTransform>();
        Transform? sourceLayout = controls?.transform.Find("Layout");
        if (controls == null || sourceRect == null || sourceLayout == null ||
            controls.transform.parent == null)
            throw new InvalidOperationException("The game's Controls panel layout is unavailable.");

        GameObject root = new(name,
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

            GameObject layout = UnityEngine.Object.Instantiate(sourceLayout.gameObject,
                root.transform, false);
            Transform? table = layout.transform.Find("Table");
            Transform? content = table?.Find("Content") ??
                table?.Find("BopItAccess Controls Viewport/Content");
            TMP_Text? title = table?.Find("Title")?.GetComponent<TMP_Text>();
            if (table == null || content == null || title == null)
                throw new InvalidOperationException("The cloned Controls layout is incomplete.");

            for (int i = 0; i < content.childCount; i++)
                content.GetChild(i).gameObject.SetActive(false);
            Transform? prompt = controls.transform.Find("ControlPrompt");
            if (prompt != null)
                UnityEngine.Object.Instantiate(prompt.gameObject, root.transform, false);

            ScrollRect? scroll = null;
            RectTransform? tableRect = table.GetComponent<RectTransform>();
            RectTransform? contentRect = content.GetComponent<RectTransform>();
            if (tableRect != null && contentRect != null)
            {
                scroll = tableRect.GetComponent<ScrollRect>();
                if (scroll == null)
                    scroll = AddSpeechViewport(tableRect, contentRect,
                        name + " Viewport");
            }
            return (root, panel, content, title, scroll);
        }
        catch
        {
            UnityEngine.Object.Destroy(root);
            throw;
        }
    }

    private void OpenGuideTopic(int index)
    {
        if (!_guideOpen || _guidePageOpen || !_guideInputReady ||
            index < 0 || index >= _guideTopics.Count ||
            _guidePagePanel == null || _guidePageRoot == null ||
            _guideTopicsPanel == null || _mainMenu?.panels == null ||
            _mainMenu.panels.Count == 0 ||
            _mainMenu.panels.Peek().GetInstanceID() != _guideTopicsPanel.GetInstanceID())
            return;
        try
        {
            _guideTopicIndex = index;
            _guideLineIndex = 0;
            _guideColumnIndex = 0;
            _guideLastLineDirection = 1;
            _guideTopicsPanel.lastSelectedButton = _guideTopicRows[index].gameObject;
            _guideTopicsPanel.Hide();
            _guidePageRoot.SetActive(true);
            _guidePagePanel.lastSelectedButton = null;
            _guidePagePanel.firstSelectedButton = _guideRepeatRow?.gameObject;
            if (_guidePageTitle != null)
                SetClonedLabel(_guidePageTitle, _guideTopics[index].Title);
            RefreshGuidePageText();
            _guidePagePanel.Show();
            _mainMenu.panels.Push(_guidePagePanel);
            _guidePageOpen = true;
            _guideLastPageFocus = null;
            GuideGateOpeningInput();
            WriteStatus("Opened guide topic: " + _guideTopics[index].Title + ".");
        }
        catch (Exception ex)
        {
            _guidePageOpen = false;
            WriteStatus("Guide topic could not be opened: " + ex);
            try
            {
                if (_mainMenu.panels.Count > 0 && _mainMenu.panels.Peek() == _guidePagePanel)
                    _mainMenu.panels.Pop();
                if (_guidePagePanel != null)
                    _guidePagePanel.Hide();
                if (_guidePageRoot != null)
                    _guidePageRoot.SetActive(false);
                _guideTopicsPanel.Show();
                GuideGateOpeningInput();
            }
            catch (Exception recoveryError)
            {
                WriteStatus("Guide topic recovery failed: " + recoveryError.Message);
                CloseGuideAfterPanelExit();
            }
            QueueSpeech(L("The user's guide could not be displayed."));
        }
    }

    private void OnGuideTopicSubmitted()
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null)
            return;
        for (int i = 0; i < _guideTopicRows.Count; i++)
        {
            SettingsButton row = _guideTopicRows[i];
            if (row == null) continue;
            if (selected.GetInstanceID() == row.gameObject.GetInstanceID() ||
                selected.transform.IsChildOf(row.transform))
            {
                OpenGuideTopic(i);
                return;
            }
        }
    }

    private void GuideGateOpeningInput()
    {
        _guideOpenedFrame = Time.frameCount;
        _guideInputReady = false;
        _guideMoveDirection = 0;
        _guideNextMoveAt = 0;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private void UpdateGuideUi()
    {
        if (!_guideOpen)
            return;
        try
        {
            if (_guideTopicsRoot == null || _guideTopicsPanel == null ||
                _mainMenu?.panels == null || _mainMenu.panels.Count == 0)
            {
                CloseGuideAfterPanelExit();
                return;
            }
            Panel top = _mainMenu.panels.Peek();
            if (top == null || _guideTopicRows.Count == 0 ||
                _guideTopicIndex < 0 || _guideTopicIndex >= _guideTopicRows.Count)
            {
                CloseGuideAfterPanelExit();
                return;
            }
            if (_guidePageOpen && _guidePagePanel != null &&
                top.GetInstanceID() == _guideTopicsPanel.GetInstanceID())
            {
                _guidePageOpen = false;
                _guidePageRoot?.SetActive(false);
                _guideLastPageFocus = null;
                _guideLastTopicRowId = 0;
                _guideTopicsPanel.firstSelectedButton =
                    _guideTopicRows[_guideTopicIndex].gameObject;
                GuideGateOpeningInput();
            }
            else if (top.GetInstanceID() != _guideTopicsPanel.GetInstanceID() &&
                (_guidePagePanel == null ||
                 top.GetInstanceID() != _guidePagePanel.GetInstanceID()))
            {
                CloseGuideAfterPanelExit();
                return;
            }

            if (!_guideInputReady && Time.frameCount > _guideOpenedFrame &&
                !IsUiSubmitHeld())
            {
                _guideInputReady = true;
                GameObject? selected = EventSystem.current?.currentSelectedGameObject;
                if (_guidePageOpen)
                {
                    if (_guideRepeatRow != null &&
                        (selected == null || !selected.transform.IsChildOf(_guidePageRoot!.transform)))
                        EventSystem.current?.SetSelectedGameObject(_guideRepeatRow.gameObject);
                }
                else if (selected == null ||
                    !selected.transform.IsChildOf(_guideTopicsRoot.transform))
                    EventSystem.current?.SetSelectedGameObject(
                        _guideTopicRows[_guideTopicIndex].gameObject);
            }

            if (_guidePageOpen && _guideInputReady)
                UpdateGuideLineNavigation();
            else if (!_guidePageOpen)
                ScrollSelectedRowIntoView(_guideTopicsScroll);
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextGuideErrorAt)
            {
                WriteStatus("Guide UI update failed: " + ex);
                _nextGuideErrorAt = now + 5000;
            }
        }
    }

    private void CloseGuideAfterPanelExit()
    {
        _guideOpen = false;
        _guidePageOpen = false;
        _guideInputReady = false;
        if (_guideTopicsRoot != null) _guideTopicsRoot.SetActive(false);
        if (_guidePageRoot != null) _guidePageRoot.SetActive(false);
        _guideLastTopicRowId = 0;
        _guideLastPageFocus = null;
        SetGuideMusicFilter(false);
        WriteStatus("Closed user's guide.");
    }

    private bool ReadGuideFocus()
    {
        if (!_guideOpen)
            return false;
        if (_guidePageOpen)
        {
            if (_guidePagePanel == null || !IsHintPanelVisible(_guidePagePanel))
                return true;
            GameObject? selected = EventSystem.current?.currentSelectedGameObject;
            if (selected == null || _guidePageRoot == null ||
                !selected.transform.IsChildOf(_guidePageRoot.transform))
                return true;
            string key = _guideTopicIndex + ":" + _guideLineIndex + ":" +
                _guideColumnIndex;
            if (!string.Equals(key, _guideLastPageFocus, StringComparison.Ordinal))
            {
                _guideLastPageFocus = key;
                string prefix = _guideLineIndex == 0
                    ? L("User's guide") + ". "
                    : string.Empty;
                QueueFocusSpeech(prefix + CurrentGuideLineSpeech());
            }
            return true;
        }

        if (_guideTopicsPanel == null || !IsHintPanelVisible(_guideTopicsPanel))
            return true;
        GameObject? focus = EventSystem.current?.currentSelectedGameObject;
        if (focus == null)
            return true;
        for (int i = 0; i < _guideTopicRows.Count; i++)
        {
            SettingsButton row = _guideTopicRows[i];
            if (row == null) continue;
            if (!focus.transform.IsChildOf(row.transform) &&
                focus.GetInstanceID() != row.gameObject.GetInstanceID())
                continue;
            _guideTopicIndex = i;
            int id = row.GetInstanceID();
            if (id != _guideLastTopicRowId)
            {
                _guideLastTopicRowId = id;
                string text = WithMenuIndex(WithControlType(_guideTopics[i].Title,
                    "button"), i, _guideTopics.Count);
                if (i == 0)
                    text = L("User's guide") + ". " + L("Topics") +
                        ". " + text;
                QueueFocusSpeech(text);
            }
            return true;
        }
        return true;
    }

    private (string Key, string Hint)? GetGuideHintContext()
    {
        if (!_guideOpen)
            return null;
        string hint = _guidePageOpen
            ? HintNavigation(true, L("read previous or next line")) + " " +
              HintNavigation(false, L("change column in a table")) + " " +
              UiBackHint(L("return to guide topics"))
            : HintNavigation(true, L("choose a guide topic")) + " " +
              UiSubmitHint(L("open topic")) + " " +
              UiBackHint(L("leave the user's guide"));
        return (_guidePageOpen ? "Guide:Page" : "Guide:Topics",
            WithGlobalControlHints(hint));
    }

    private void UpdateGuideLineNavigation()
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        InputAction? move = module?.move?.action;
        if (move == null || !move.enabled)
            move = FindHintAction("UI", "Navigate");
        Vector2 axis = move != null && move.enabled
            ? move.ReadValue<Vector2>() : Vector2.zero;
        float horizontal = Mathf.Abs(axis.x);
        float vertical = Mathf.Abs(axis.y);
        int direction = horizontal < 0.5f && vertical < 0.5f
            ? 0
            : vertical >= horizontal
                ? (axis.y > 0 ? 1 : 2)
                : (axis.x > 0 ? 4 : 3);
        if (direction == 0)
        {
            _guideMoveDirection = 0;
            return;
        }
        long now = Environment.TickCount64;
        if (direction == _guideMoveDirection && now < _guideNextMoveAt)
            return;
        bool first = direction != _guideMoveDirection;
        _guideMoveDirection = direction;
        _guideNextMoveAt = now + (first ? 350 : 150);

        GuideTopic topic = _guideTopics[_guideTopicIndex];
        if (topic.Lines.Count == 0)
            return;
        if (direction == 1 || direction == 2)
        {
            int next = Math.Clamp(_guideLineIndex + (direction == 1 ? -1 : 1),
                0, topic.Lines.Count - 1);
            if (next == _guideLineIndex)
                return;
            GuideLine oldLine = topic.Lines[_guideLineIndex];
            GuideLine newLine = topic.Lines[next];
            bool stayingInTable = oldLine.Cells != null &&
                newLine.Cells != null &&
                string.Equals(oldLine.TableCaption, newLine.TableCaption,
                    StringComparison.Ordinal);
            _guideLastLineDirection = direction == 1 ? -1 : 1;
            _guideLineIndex = next;
            _guideColumnIndex = stayingInTable
                ? Math.Clamp(_guideColumnIndex, 0, newLine.Cells!.Count - 1)
                : 0;
        }
        else
        {
            GuideLine line = topic.Lines[_guideLineIndex];
            if (line.Cells == null || line.Cells.Count <= 1)
                return;
            int next = Math.Clamp(_guideColumnIndex + (direction == 3 ? -1 : 1),
                0, line.Cells.Count - 1);
            if (next == _guideColumnIndex)
                return;
            _guideColumnIndex = next;
        }
        RefreshGuidePageText();
        RecordButtonHintUiActivity(now);
    }

    private void RefreshGuidePageText()
    {
        if (_guidePageText == null || _guideTopicIndex >= _guideTopics.Count)
            return;
        GuideTopic topic = _guideTopics[_guideTopicIndex];
        _guidePageText.text = topic.Lines.Count == 0
            ? L("This topic has no readable text. Press Back to return to topics.")
            : CurrentGuideLineText(forSpeech: false);
        if (_guidePageScroll != null)
            _guidePageScroll.verticalNormalizedPosition = 1f;
    }

    private string CurrentGuideLineSpeech() => CurrentGuideLineText(forSpeech: true);

    private string CurrentGuideLineText(bool forSpeech)
    {
        GuideTopic topic = _guideTopics[_guideTopicIndex];
        if (topic.Lines.Count == 0)
            return L("This topic has no readable text.");
        GuideLine line = topic.Lines[_guideLineIndex];
        if (line.Cells != null && line.Cells.Count > 0)
        {
            int column = Math.Clamp(_guideColumnIndex, 0, line.Cells.Count - 1);
            string heading = line.Headers != null && column < line.Headers.Count
                ? line.Headers[column] : string.Format(
                    CultureInfo.CurrentCulture, L("Column {0}"),
                    LocalizedIndexNumber(column + 1));
            string rowName = line.Cells[0].Length > 0
                ? line.Cells[0] : string.Format(
                    CultureInfo.CurrentCulture, L("Row {0}"),
                    LocalizedIndexNumber(line.TableRow));
            string rowLabel = _indexingEnabled
                ? string.Format(CultureInfo.CurrentCulture,
                    L("{0}, {1} of {2}"), rowName,
                    LocalizedIndexNumber(line.TableRow),
                    LocalizedIndexNumber(line.TableRows))
                : rowName;
            return column == 0
                ? rowLabel
                : string.Format(CultureInfo.CurrentCulture,
                    L("{0}. {1}: {2}"), rowLabel, heading,
                    line.Cells[column]);
        }
        string plainText = line.Text;
        if (line.IsTableMarker)
            return _guideLastLineDirection < 0
                ? line.ReverseTableText : plainText;
        if (forSpeech && _indexingEnabled && _filterCapitalisationEnabled)
            plainText = FormatGuideTextBeforeIndex(plainText);
        return _indexingEnabled
            ? string.Format(CultureInfo.CurrentCulture,
                L("{0} Line {1} of {2}."), plainText,
                LocalizedIndexNumber(_guideLineIndex + 1),
                LocalizedIndexNumber(topic.Lines.Count))
            : plainText;
    }

    private void RepeatGuideLine()
    {
        if (_guideOpen && _guidePageOpen && _guideInputReady)
            QueueSpeech(CurrentGuideLineSpeech());
    }

    private void DestroyGuidePanels()
    {
        if (_guideTopicsRoot != null) _guideTopicsRoot.SetActive(false);
        if (_guidePageRoot != null) _guidePageRoot.SetActive(false);
        if (_guideTopicsRoot != null)
            UnityEngine.Object.Destroy(_guideTopicsRoot);
        if (_guidePageRoot != null)
            UnityEngine.Object.Destroy(_guidePageRoot);
        _guideTopicsRoot = null;
        _guidePageRoot = null;
        _guideTopicsPanel = null;
        _guidePagePanel = null;
        _guideTopicsScroll = null;
        _guidePageScroll = null;
        _guidePageText = null;
        _guidePageTitle = null;
        _guideRepeatRow = null;
        _guideTopicRows.Clear();
        _guideTopics.Clear();
    }

    private sealed record GuideTopic(string Title, List<GuideLine> Lines);

    private sealed record GuideLine(string Text, List<string>? Cells = null,
        List<string>? Headers = null, string TableCaption = "",
        int TableRow = 0, int TableRows = 0, bool IsTableMarker = false,
        string ReverseTableText = "");

    private sealed class GuideHtmlNode
    {
        internal GuideHtmlNode(string name, string attributes = "", string value = "")
        {
            Name = name;
            Attributes = attributes;
            Value = value;
        }
        internal string Name { get; }
        internal string Attributes { get; }
        internal string Value { get; }
        internal List<GuideHtmlNode> Children { get; } = new();
    }

    private static readonly Regex GuideHtmlTokens = new(
        @"<!--.*?-->|<![^>]*>|<(?<close>/)?(?<tag>[A-Za-z][A-Za-z0-9-]*)(?<attrs>[^>]*)>|(?<text>[^<]+)",
        RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(2));
    private static readonly Regex GuideSpace = new(@"\s+", RegexOptions.Compiled, TimeSpan.FromSeconds(2));

    private List<GuideTopic> ReadGuideDocument(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Guide HTML is missing.", path);
        using FileStream file = File.OpenRead(path);
        if (file.Length > MaximumGuideBytes)
            throw new FormatException("Guide HTML exceeds the four-megabyte size limit.");
        using var reader = new StreamReader(file);
        var source = new System.Text.StringBuilder();
        char[] buffer = new char[8192];
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (source.Length + read > MaximumGuideBytes)
                throw new FormatException("Guide HTML grew beyond its size limit while reading.");
            source.Append(buffer, 0, read);
        }
        string html = source.ToString();
        GuideHtmlNode document = ParseGuideHtml(html);
        GuideHtmlNode? contents = FindGuideNode(document, node =>
            node.Name == "nav" &&
            (string.Equals(GuideAttribute(node, "id"), "contents-nav",
                StringComparison.Ordinal) ||
             GuideDescendants(node).Any(child => child.Name == "h2" &&
                 string.Equals(GuideAttribute(child, "id"), "contents",
                     StringComparison.Ordinal))));
        if (contents == null)
            throw new FormatException("Guide table of contents was not found.");

        var sections = new Dictionary<string, GuideHtmlNode>(StringComparer.Ordinal);
        foreach (GuideHtmlNode node in GuideDescendants(document))
            if (node.Name == "section" && GuideAttribute(node, "id") is string sectionId)
                sections.TryAdd(sectionId, node);
        var seenTopics = new HashSet<string>(StringComparer.Ordinal);
        int lineCount = 0;
        var topics = new List<GuideTopic>();
        foreach (GuideHtmlNode link in GuideDescendants(contents).Where(node =>
                     node.Name == "a"))
        {
            string? href = GuideAttribute(link, "href");
            if (href == null || !href.StartsWith('#'))
                continue;
            string id = WebUtility.HtmlDecode(href[1..]);
            if (!seenTopics.Add(id) || !sections.TryGetValue(id, out GuideHtmlNode? section))
                continue;
            if (topics.Count >= 256)
                throw new FormatException("Guide table of contents exceeds 256 topics.");
            string title = GuideText(link);
            if (title.Length == 0)
                continue;
            var lines = new List<GuideLine>();
            AppendGuideLines(section, lines);
            lineCount += lines.Count;
            if (lineCount > MaximumGuideLines)
                throw new FormatException("Guide exceeds the supported number of reading lines.");
            topics.Add(new GuideTopic(title, lines));
        }
        return topics;
    }

    private static GuideHtmlNode ParseGuideHtml(string html)
    {
        GuideHtmlNode root = new("document");
        var stack = new Stack<GuideHtmlNode>();
        stack.Push(root);
        int nodes = 0;
        foreach (Match token in GuideHtmlTokens.Matches(html))
        {
            if (++nodes > MaximumGuideNodes)
                throw new FormatException("Guide HTML exceeds its node limit.");
            if (token.Groups["text"].Success)
            {
                stack.Peek().Children.Add(new GuideHtmlNode("#text", value:
                    token.Groups["text"].Value));
                continue;
            }
            if (!token.Groups["tag"].Success)
                continue;
            string tag = token.Groups["tag"].Value.ToLowerInvariant();
            if (token.Groups["close"].Success)
            {
                // Ignore unmatched closing tags instead of discarding the
                // whole surrounding document (for example stray </span>).
                if (!stack.Any(node => node.Name == tag))
                    continue;
                while (stack.Count > 1 && stack.Peek().Name != tag)
                    stack.Pop();
                if (stack.Count > 1)
                    stack.Pop();
                continue;
            }
            string attributes = token.Groups["attrs"].Value;
            GuideHtmlNode node = new(tag, attributes);
            stack.Peek().Children.Add(node);
            if (!attributes.TrimEnd().EndsWith('/') &&
                tag is not ("br" or "hr" or "img" or "input" or "meta" or
                    "link" or "source" or "area" or "wbr"))
            {
                if (stack.Count >= MaximumGuideDepth)
                    throw new FormatException("Guide HTML is nested too deeply.");
                stack.Push(node);
            }
        }
        return root;
    }

    private static string? GuideAttribute(GuideHtmlNode node, string name)
    {
        Match match = Regex.Match(node.Attributes,
            @"(?:^|\s)" + Regex.Escape(name) +
            @"\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^\s>]+))",
            RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
        return match.Success ? WebUtility.HtmlDecode(match.Groups["value"].Value)
            : null;
    }

    private static GuideHtmlNode? FindGuideNode(GuideHtmlNode root,
        Func<GuideHtmlNode, bool> predicate) => predicate(root)
            ? root : GuideDescendants(root).FirstOrDefault(predicate);

    private static IEnumerable<GuideHtmlNode> GuideDescendants(GuideHtmlNode root)
    {
        var pending = new Stack<GuideHtmlNode>();
        PushGuideChildren(root, pending);
        while (pending.Count > 0)
        {
            GuideHtmlNode node = pending.Pop();
            yield return node;
            PushGuideChildren(node, pending);
        }
    }

    private static void PushGuideChildren(GuideHtmlNode node,
        Stack<GuideHtmlNode> pending)
    {
        // Reverse push preserves HTML reading order without recursion.
        for (int i = node.Children.Count - 1; i >= 0; i--)
            pending.Push(node.Children[i]);
    }

    private static string GuideText(GuideHtmlNode node)
    {
        var raw = new System.Text.StringBuilder();
        AppendGuideText(node, raw);
        return GuideSpace.Replace(WebUtility.HtmlDecode(raw.ToString()), " ").Trim();
    }

    private static void AppendGuideText(GuideHtmlNode root,
        System.Text.StringBuilder text)
    {
        var pending = new Stack<GuideHtmlNode>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            GuideHtmlNode node = pending.Pop();
            if (node.Name == "#text")
                text.Append(node.Value);
            else if (node.Name == "br")
                text.Append(' ');
            else if (node.Name == "img")
                text.Append(GuideAttribute(node, "alt") ?? string.Empty);
            PushGuideChildren(node, pending);
        }
    }

    private void AppendGuideLines(GuideHtmlNode root, List<GuideLine> lines)
    {
        var pending = new Stack<GuideHtmlNode>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            GuideHtmlNode node = pending.Pop();
            if (node.Name == "table")
                AppendGuideTable(node, lines);
            else if (node.Name is "h2" or "h3" or "h4" or "p" or "li" or "pre" or "blockquote")
            {
                string value = GuideText(node);
                if (value.Length > 0)
                    AppendGuideWrappedText(node.Name == "li"
                        ? L("Bullet") + ". " + value : value, lines);
            }
            else
                PushGuideChildren(node, pending);
            if (lines.Count > MaximumGuideLines)
                throw new FormatException("Guide topic exceeds its reading-line limit.");
        }
    }

    private void AppendGuideWrappedText(string text,
        List<GuideLine> lines)
    {
        // A paragraph can span much of a visual page. Short speech lines let
        // native Up/Down navigation advance through it at a reading pace.
        // Japanese and Chinese commonly have no spaces between words. Count
        // grapheme clusters rather than splitting only at whitespace there.
        // Korean normally has word spaces, so use the word-aware path below.
        if (CurrentGameLocale is "ja" or "zh")
        {
            const int textElementsPerLine = 58;
            var currentCjk = new System.Text.StringBuilder();
            int elements = 0;
            TextElementEnumerator iterator = StringInfo.GetTextElementEnumerator(text);
            while (iterator.MoveNext())
            {
                string element = iterator.GetTextElement();
                if (string.IsNullOrWhiteSpace(element))
                {
                    if (currentCjk.Length == 0 || currentCjk[^1] == ' ')
                        continue;
                    element = " ";
                }
                if (elements >= textElementsPerLine)
                {
                    AddGuideLine(lines, new GuideLine(currentCjk.ToString().TrimEnd()));
                    currentCjk.Clear();
                    elements = 0;
                    if (element == " ")
                        continue;
                }
                currentCjk.Append(element);
                elements++;
            }
            if (currentCjk.Length > 0)
                AddGuideLine(lines, new GuideLine(currentCjk.ToString().TrimEnd()));
            return;
        }
        int lineLength = CurrentGameLocale == "ko" ? 60 : 120;
        var current = new System.Text.StringBuilder();
        int offset = 0;
        while (offset < text.Length)
        {
            while (offset < text.Length && char.IsWhiteSpace(text[offset])) offset++;
            int start = offset;
            while (offset < text.Length && !char.IsWhiteSpace(text[offset])) offset++;
            if (offset == start) break;
            string value = text[start..offset];
            if (current.Length > 0 && current.Length + 1 + value.Length > lineLength)
            {
                AddGuideLine(lines, new GuideLine(current.ToString()));
                current.Clear();
            }
            if (value.Length > lineLength)
            {
                // Long URLs or unspaced text must not create a huge native
                // text row. Split on grapheme boundaries, preserving content.
                TextElementEnumerator iterator = StringInfo.GetTextElementEnumerator(value);
                int elements = 0;
                while (iterator.MoveNext())
                {
                    if (elements == lineLength)
                    {
                        AddGuideLine(lines, new GuideLine(current.ToString()));
                        current.Clear();
                        elements = 0;
                    }
                    current.Append(iterator.GetTextElement());
                    elements++;
                }
            }
            else
            {
                if (current.Length > 0) current.Append(' ');
                current.Append(value);
            }
        }
        if (current.Length > 0)
            AddGuideLine(lines, new GuideLine(current.ToString()));
    }

    private static void AddGuideLine(List<GuideLine> lines, GuideLine line)
    {
        if (lines.Count >= MaximumGuideLines)
            throw new FormatException("Guide topic exceeds its reading-line limit.");
        lines.Add(line);
    }

    private void AppendGuideTable(GuideHtmlNode table,
        List<GuideLine> lines)
    {
        GuideHtmlNode? captionNode = FindGuideNode(table,
            node => node.Name == "caption");
        string caption = captionNode == null ? "" : GuideText(captionNode);
        List<GuideHtmlNode> rows = GuideDescendants(table).Where(node =>
            node.Name == "tr").ToList();
        List<string>? headers = null;
        var dataRows = new List<List<string>>();
        foreach (GuideHtmlNode row in rows)
        {
            List<GuideHtmlNode> cellNodes = row.Children.Where(node =>
                node.Name is "th" or "td").ToList();
            List<string> cells = cellNodes.Select(GuideText).ToList();
            if (cells.Count == 0)
                continue;
            if (headers == null && cellNodes.All(node => node.Name == "th"))
            {
                headers = cells;
                continue;
            }
            dataRows.Add(cells);
        }
        int columns = Math.Max(headers?.Count ?? 0,
            dataRows.Count == 0 ? 0 : dataRows.Max(row => row.Count));
        string entry = caption.Length > 0
            ? string.Format(CultureInfo.CurrentCulture,
                L("Table: {0}. Rows: {1}. Columns: {2}."),
                caption, LocalizedIndexNumber(dataRows.Count),
                LocalizedIndexNumber(columns))
            : string.Format(CultureInfo.CurrentCulture,
                L("Table. Rows: {0}. Columns: {1}."),
                LocalizedIndexNumber(dataRows.Count),
                LocalizedIndexNumber(columns));
        string exit = caption.Length > 0
            ? string.Format(CultureInfo.CurrentCulture,
                L("End of table: {0}."), caption)
            : L("End of table.");
        AddGuideLine(lines, new GuideLine(entry, TableCaption: caption,
            IsTableMarker: true, ReverseTableText: exit));
        for (int index = 0; index < dataRows.Count; index++)
            AddGuideLine(lines, new GuideLine("", dataRows[index], headers, caption,
                index + 1, dataRows.Count));
        AddGuideLine(lines, new GuideLine(exit, TableCaption: caption,
            IsTableMarker: true, ReverseTableText: entry));
    }
}
