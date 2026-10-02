using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private CreditsContent? _creditsContent;
    private readonly List<string> _creditsLines = new();
    private readonly List<TMP_Text> _creditsLineLabels = new();
    private readonly HashSet<int> _creditsAutoAnnounced = new();
    private long _nextCreditsSearchAt;
    private int _creditsPanelId;
    private int _creditsLineIndex;
    private int _creditsMoveDirection;
    private long _nextCreditsMoveAt;
    private string? _creditsPopulationSignature;
    private long _creditsPopulationStableSince;
    private long _creditsAutoStartAt;
    private long _nextCreditsVisualScanAt;
    private long _creditsLastScrollMotionAt;
    private float _creditsLastNormalizedPosition;
    private float _creditsLastScrollContentY;
    private float _creditsLastContainerY;
    private bool _creditsScrollPositionSampled;
    private bool _creditsObservedScrollMotion;
    private bool _creditsAutoReading;
    private bool _creditsSingleFallbackSpoken;
    private bool _creditsIntroductionSpoken;

    // The credit entries are generated as separate GameObjects. Read their
    // labels independently of CreditsContent's existing automatic scroll.
    private bool ReadCreditsFocus()
    {
        if (_mainMenu == null)
        {
            ResetCreditsFocus();
            long now = Environment.TickCount64;
            if (now < _nextCreditsSearchAt)
                return false;

            _nextCreditsSearchAt = now + 500;
            _mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_mainMenu == null)
                return false;
        }

        Panel? panel = _mainMenu.creditsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetCreditsFocus();
            return false;
        }

        int panelId = panel.GetInstanceID();
        if (panelId != _creditsPanelId)
        {
            ResetCreditsFocus();
            _creditsPanelId = panelId;
        }

        if (_creditsContent == null)
            _creditsContent = panel.GetComponentInChildren<CreditsContent>(true);

        if (!_creditsIntroductionSpoken)
        {
            // PopulateCredits can run after the panel becomes visible. Wait
            // until the game's own localized labels exist before speaking.
            if (_creditsContent == null || !CollectCreditsLines(_creditsContent))
                return true;

            _creditsIntroductionSpoken = true;
            _creditsLineIndex = 0;
            _creditsMoveDirection = 0;
            ReadCreditsMoveDirection(out bool navigationAvailable);
            _creditsAutoReading = !navigationAvailable;
            _creditsAutoStartAt = Environment.TickCount64;
            if (_creditsAutoReading)
                QueueFocusSpeech(L("Credits."));
            else
                QueueFocusSpeech(LF("Credits. {0}.",
                    WithCreditLineType(_creditsLines[0], 0, _creditsLines.Count)));
            WriteStatus($"Credits panel is visible; captured {_creditsLines.Count} spoken lines.");
            return true;
        }

        if (_creditsAutoReading)
        {
            ReadCreditsAsTheyAppear(_creditsContent!);
            return true;
        }

        int direction = ReadCreditsMoveDirection(out _);
        long currentTime = Environment.TickCount64;
        if (direction == 0)
        {
            _creditsMoveDirection = 0;
            return true;
        }

        if (direction != _creditsMoveDirection || currentTime >= _nextCreditsMoveAt)
        {
            bool changedDirection = direction != _creditsMoveDirection;
            _creditsMoveDirection = direction;
            _nextCreditsMoveAt = currentTime + (changedDirection ? 430 : 180);
            int nextIndex = Math.Clamp(_creditsLineIndex + direction, 0, _creditsLines.Count - 1);
            if (nextIndex != _creditsLineIndex)
            {
                _creditsLineIndex = nextIndex;
                QueueFocusSpeech(WithCreditLineType(_creditsLines[_creditsLineIndex],
                    _creditsLineIndex, _creditsLines.Count));
            }
        }

        return true;
    }

    private bool CollectCreditsLines(CreditsContent content)
    {
        _creditsLines.Clear();
        _creditsLineLabels.Clear();
        var seenLabels = new HashSet<int>();

        // Hierarchy order is the displayed reading order, even if `entries`
        // includes nested objects as separate items.
        if (content.container != null)
            AddCreditObjectLines(content.container, seenLabels);

        var entries = content.entries;
        if (_creditsLines.Count == 0 && entries != null)
            for (int index = 0; index < entries.Count; index++)
                AddCreditObjectLines(entries[index], seenLabels);

        if (_creditsLines.Count == 0)
            return false;

        // PopulateCredits creates its entries synchronously, but this stable
        // window also allows localized TMP labels to finish updating.
        string signature = $"{entries?.Count ?? 0}:{string.Join("\u001f", _creditsLines)}";
        long now = Environment.TickCount64;
        if (!string.Equals(signature, _creditsPopulationSignature, StringComparison.Ordinal))
        {
            _creditsPopulationSignature = signature;
            _creditsPopulationStableSince = now;
            return false;
        }

        return now - _creditsPopulationStableSince >= 300;
    }

    private void AddCreditObjectLines(GameObject? entry, HashSet<int> seenLabels)
    {
        if (entry == null)
            return;

        // Offscreen or animating entries may be inactive. Their generated
        // text still belongs in the full reading order.
        var labels = entry.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text label in labels)
        {
            if (label == null || !seenLabels.Add(label.GetInstanceID()))
                continue;

            // The game can put several credited names in one text object.
            // Preserve those as separate speech stops.
            string raw = System.Text.RegularExpressions.Regex.Replace(
                label.text ?? string.Empty, @"<br\s*/?>", "\n",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (string part in raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string? line = CleanSpeechValue(part);
                if (line != null)
                {
                    _creditsLines.Add(line);
                    _creditsLineLabels.Add(label);
                }
            }
        }
    }

    private void ReadCreditsAsTheyAppear(CreditsContent content)
    {
        if (_creditsSingleFallbackSpoken)
            return;

        if (!content.enableAutoScroll)
        {
            SpeakAllCreditsAsLastResort("Credits auto-scroll is disabled");
            return;
        }

        long now = Environment.TickCount64;
        if (now < _nextCreditsVisualScanAt)
            return;
        _nextCreditsVisualScanAt = now + 120;

        ScrollRect? scroll = content.scrollRect;
        RectTransform? viewport = scroll?.viewport;
        if (viewport == null && scroll != null)
            viewport = scroll.GetComponent<RectTransform>();

        bool viewportUsable = viewport != null &&
            viewport.rect.width > 1f && viewport.rect.height > 1f;
        if (viewportUsable)
        {
            ObserveCreditsScroll(content, scroll!, now);
            Rect visible = viewport!.rect;
            for (int index = 0; index < _creditsLines.Count; index++)
            {
                TMP_Text label = _creditsLineLabels[index];
                if (_creditsAutoAnnounced.Contains(index) || label == null || !label.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    viewport, label.rectTransform);
                if (bounds.max.y < visible.yMin || bounds.min.y > visible.yMax)
                    continue;

                _creditsAutoAnnounced.Add(index);
                QueueSequentialSpeech(WithCreditLineType(_creditsLines[index], index,
                    _creditsLines.Count));
            }
        }

        if (!viewportUsable)
        {
            // A missing or zero-sized viewport is a genuine layout failure.
            if (now - _creditsAutoStartAt >= 2000)
                CompleteCreditsVisualFallback("Credits viewport could not be read");
            return;
        }

        // A valid viewport can have a long empty lead-in. Keep watching while
        // it moves; only conclude that visual reading has stalled after the
        // scroll has stopped, or after a long period with no movement at all.
        bool scrollStalled = _creditsObservedScrollMotion
            ? now - _creditsLastScrollMotionAt >= 5000 && now - _creditsAutoStartAt >= 10000
            : now - _creditsAutoStartAt >= 30000;
        if (scrollStalled && _creditsAutoAnnounced.Count < _creditsLines.Count)
            CompleteCreditsVisualFallback("Credits visual reading stopped before every line appeared");
    }

    private void ObserveCreditsScroll(CreditsContent content, ScrollRect scroll, long now)
    {
        float normalized = scroll.verticalNormalizedPosition;
        float scrollContentY = scroll.content == null ? 0 : scroll.content.anchoredPosition.y;
        float containerY = content.container == null ? 0 : content.container.transform.localPosition.y;
        if (_creditsScrollPositionSampled &&
            (Math.Abs(normalized - _creditsLastNormalizedPosition) > 0.001f ||
             Math.Abs(scrollContentY - _creditsLastScrollContentY) > 0.1f ||
             Math.Abs(containerY - _creditsLastContainerY) > 0.1f))
        {
            _creditsObservedScrollMotion = true;
            _creditsLastScrollMotionAt = now;
        }

        _creditsLastNormalizedPosition = normalized;
        _creditsLastScrollContentY = scrollContentY;
        _creditsLastContainerY = containerY;
        _creditsScrollPositionSampled = true;
    }

    private void CompleteCreditsVisualFallback(string reason)
    {
        if (_creditsAutoAnnounced.Count == 0)
        {
            SpeakAllCreditsAsLastResort(reason);
            return;
        }

        int missing = 0;
        for (int index = 0; index < _creditsLines.Count; index++)
        {
            if (!_creditsAutoAnnounced.Add(index))
                continue;
            QueueSequentialSpeech(WithCreditLineType(_creditsLines[index], index,
                _creditsLines.Count));
            missing++;
        }
        WriteStatus($"{reason}; queued {missing} remaining credit lines.");
    }

    private void SpeakAllCreditsAsLastResort(string reason)
    {
        _creditsSingleFallbackSpoken = true;
        QueueSpeech(string.Join(". ", _creditsLines.Select((line, index) =>
            WithCreditLineType(line, index, _creditsLines.Count))), false);
        WriteStatus($"{reason}; sent complete credits as one announcement.");
    }

    private string WithCreditLineType(string line, int index, int count) =>
        WithMenuIndex(WithControlType(line, "list item"), index, count);

    private static int ReadCreditsMoveDirection(out bool available)
    {
        // Use the action that drives the game's EventSystem. This observes
        // remapped keyboard/controller navigation without consuming it.
        EventSystem? eventSystem = EventSystem.current;
        InputSystemUIInputModule? inputModule = eventSystem == null
            ? null : eventSystem.GetComponent<InputSystemUIInputModule>();
        InputAction? move = inputModule?.move?.action;
        available = move != null && move.enabled;
        if (!available)
            return 0;

        Vector2 value = move!.ReadValue<Vector2>();
        if (Math.Abs(value.y) < 0.5f || Math.Abs(value.y) < Math.Abs(value.x))
            return 0;

        return value.y > 0 ? -1 : 1;
    }

    private void ResetCreditsFocus()
    {
        _creditsContent = null;
        _creditsLines.Clear();
        _creditsLineLabels.Clear();
        _creditsAutoAnnounced.Clear();
        _creditsPanelId = 0;
        _creditsLineIndex = 0;
        _creditsMoveDirection = 0;
        _nextCreditsMoveAt = 0;
        _creditsPopulationSignature = null;
        _creditsPopulationStableSince = 0;
        _creditsAutoStartAt = 0;
        _nextCreditsVisualScanAt = 0;
        _creditsLastScrollMotionAt = 0;
        _creditsLastNormalizedPosition = 0;
        _creditsLastScrollContentY = 0;
        _creditsLastContainerY = 0;
        _creditsScrollPositionSampled = false;
        _creditsObservedScrollMotion = false;
        _creditsAutoReading = false;
        _creditsSingleFallbackSpoken = false;
        _creditsIntroductionSpoken = false;
    }
}
