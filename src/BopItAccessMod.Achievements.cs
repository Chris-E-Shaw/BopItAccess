using Il2Cpp;
using Il2Cppecho17.EndlessBook;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private AchievementsPanel? _achievementsPanel;
    private BookController? _achievementBookController;
    private long _nextAchievementSearchAt;
    private long _nextAchievementBookSearchAt;
    private long _nextAchievementContentReadAt;
    private long _achievementOpenedAt;
    private bool _achievementBookWasOpen;
    private bool _achievementCloseFocusHold;
    private long _achievementCloseHoldUntil;
    private bool _achievementOpeningSpoken;
    private string? _lastAchievementSpread;
    private string? _lastAchievementPage;
    private readonly List<string> _achievementLines = new();
    private int _achievementLineIndex;
    private int _achievementMoveDirection;
    private long _nextAchievementMoveAt;

    // The game has its own achievement book. Each page view holds either
    // rendered text-list entries or stickers, and the book controller turns
    // those page views on and off as the player flips pages.
    private bool ReadAchievementsFocus()
    {
        if (_achievementsPanel == null)
        {
            AnnounceAchievementBookClosed();
            ResetAchievementsFocus();
            long now = Environment.TickCount64;
            if (now < _nextAchievementSearchAt)
                return HoldAchievementCloseUntilFocusMoves();

            _nextAchievementSearchAt = now + 500;
            _achievementsPanel = UnityEngine.Object.FindFirstObjectByType<AchievementsPanel>();
            if (_achievementsPanel == null)
                return HoldAchievementCloseUntilFocusMoves();
        }

        if (!_achievementsPanel.gameObject.activeInHierarchy ||
            _achievementsPanel.achievementViewState == AchievementViewState.Hidden)
        {
            AnnounceAchievementBookClosed();
            ResetAchievementsFocus();
            return HoldAchievementCloseUntilFocusMoves();
        }

        if (_achievementsPanel.achievementViewState == AchievementViewState.VisibleToHidden)
        {
            AnnounceAchievementBookClosed();
            return HoldAchievementCloseUntilFocusMoves();
        }

        if (_achievementsPanel.achievementViewState != AchievementViewState.Visible)
            return HoldAchievementCloseUntilFocusMoves();

        if (_achievementBookController == null)
        {
            long now = Environment.TickCount64;
            if (now >= _nextAchievementBookSearchAt)
            {
                _nextAchievementBookSearchAt = now + 250;
                _achievementBookController = UnityEngine.Object.FindFirstObjectByType<BookController>();
            }
        }

        BookController? controller = _achievementBookController;
        EndlessBook? book = controller?.Book;
        if (book == null)
            return HoldAchievementCloseUntilFocusMoves();

        // Focusing Achievements in the main menu slides in a closed book
        // cover. Its panel is already Visible, but the player has not opened
        // the achievement pages. Let the main-menu reader speak its button.
        if (book.CurrentState == EndlessBook.StateEnum.ClosedFront ||
            book.CurrentState == EndlessBook.StateEnum.ClosedBack)
        {
            AnnounceAchievementBookClosed();
            return HoldAchievementCloseUntilFocusMoves();
        }

        if (book.CurrentState != EndlessBook.StateEnum.OpenFront &&
            book.CurrentState != EndlessBook.StateEnum.OpenMiddle &&
            book.CurrentState != EndlessBook.StateEnum.OpenBack)
            return HoldAchievementCloseUntilFocusMoves();

        _achievementCloseFocusHold = false;
        if (!_achievementBookWasOpen)
        {
            _achievementBookWasOpen = true;
            _achievementOpenedAt = Environment.TickCount64;
            WriteStatus("Achievements book opened; waiting for its current pages.");
        }

        if (controller!.IsFlipping || book.IsTurningPages)
        {
            SpeakAchievementOpeningIfDelayed();
            return true;
        }

        long readAt = Environment.TickCount64;
        if (readAt < _nextAchievementContentReadAt)
        {
            ReadAchievementLineNavigation(controller);
            return true;
        }
        _nextAchievementContentReadAt = readAt + 100;

        List<string> pageLines = ReadAchievementPage(controller);
        if (pageLines.Count == 0)
        {
            SpeakAchievementOpeningIfDelayed();
            return true;
        }

        string spread = $"{book.CurrentLeftPageNumber}:{book.CurrentRightPageNumber}";
        string pageKey = spread + ":" + string.Join("\u001f", pageLines);
        if (!string.Equals(pageKey, _lastAchievementPage, StringComparison.Ordinal))
        {
            bool firstPage = _lastAchievementSpread == null;
            bool sameSpread = string.Equals(spread, _lastAchievementSpread, StringComparison.Ordinal);
            string? previouslySelectedLine = _achievementLines.Count > 0 &&
                _achievementLineIndex < _achievementLines.Count
                ? _achievementLines[_achievementLineIndex] : null;
            var previousLineCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (sameSpread)
            {
                foreach (string line in _achievementLines)
                {
                    previousLineCounts.TryGetValue(line, out int count);
                    previousLineCounts[line] = count + 1;
                }
            }
            _lastAchievementSpread = spread;
            _lastAchievementPage = pageKey;
            _achievementLines.Clear();
            _achievementLines.AddRange(pageLines);
            if (sameSpread)
            {
                // Text-list entries can appear after their heading. Refresh
                // the navigation data without interrupting the opening speech.
                int existingIndex = previouslySelectedLine == null
                    ? -1 : _achievementLines.FindIndex(line =>
                        string.Equals(line, previouslySelectedLine, StringComparison.Ordinal));
                _achievementLineIndex = existingIndex >= 0
                    ? existingIndex : Math.Clamp(_achievementLineIndex, 0, _achievementLines.Count - 1);
                if (GetAchievementMoveAction(controller) == null)
                {
                    // When the book has no navigation action, text that
                    // finishes populating after the heading must still be
                    // spoken without interrupting the opening instructions.
                    for (int index = 0; index < _achievementLines.Count; index++)
                    {
                        string line = _achievementLines[index];
                        if (previousLineCounts.TryGetValue(line, out int count) && count > 0)
                        {
                            previousLineCounts[line] = count - 1;
                            continue;
                        }
                        QueueSequentialSpeech(WithAchievementLineType(line, index,
                            _achievementLines.Count));
                    }
                }
                WriteStatus($"Achievements page {spread} updated to {_achievementLines.Count} readable lines.");
                return true;
            }

            _achievementLineIndex = 0;
            _achievementMoveDirection = 0;
            if (firstPage && !_achievementOpeningSpoken)
            {
                QueueFocusSpeech("Achievements book. " +
                    WithAchievementLineType(_achievementLines[0], 0, _achievementLines.Count));
                if (GetAchievementMoveAction(controller) == null)
                {
                    for (int index = 1; index < _achievementLines.Count; index++)
                        QueueSequentialSpeech(WithAchievementLineType(_achievementLines[index], index,
                            _achievementLines.Count));
                }
            }
            else
            {
                string announcement = "Achievements page. " +
                    WithAchievementLineType(_achievementLines[0], 0, _achievementLines.Count);
                if (_achievementLines.Count > 1 && GetAchievementMoveAction(controller) == null)
                    announcement += " " + string.Join(". ",
                        _achievementLines.Skip(1).Select((line, index) =>
                            WithAchievementLineType(line, index + 1, _achievementLines.Count)));
                QueueFocusSpeech(announcement, !firstPage);
            }
            _achievementOpeningSpoken = true;
            WriteStatus($"Achievements page {book.CurrentLeftPageNumber}/{book.CurrentRightPageNumber}: {_achievementLines.Count} spoken lines.");
            return true;
        }

        ReadAchievementLineNavigation(controller);
        return true;
    }

    private void ReadAchievementLineNavigation(BookController controller)
    {
        if (_achievementLines.Count < 2)
            return;

        // This is the same UI move action the game uses for remapped menu
        // navigation. Reading it does not consume the input or move the book.
        InputAction? move = GetAchievementMoveAction(controller);
        if (move == null)
            return;

        Vector2 value = move.ReadValue<Vector2>();
        int direction = Math.Abs(value.y) < 0.5f || Math.Abs(value.y) < Math.Abs(value.x)
            ? 0 : value.y > 0 ? -1 : 1;
        if (direction == 0)
        {
            _achievementMoveDirection = 0;
            return;
        }

        long now = Environment.TickCount64;
        if (direction == _achievementMoveDirection && now < _nextAchievementMoveAt)
            return;

        bool changedDirection = direction != _achievementMoveDirection;
        _achievementMoveDirection = direction;
        _nextAchievementMoveAt = now + (changedDirection ? 430 : 180);
        int nextIndex = Math.Clamp(_achievementLineIndex + direction, 0, _achievementLines.Count - 1);
        if (nextIndex == _achievementLineIndex)
            return;

        _achievementLineIndex = nextIndex;
        QueueFocusSpeech(WithAchievementLineType(_achievementLines[nextIndex], nextIndex,
            _achievementLines.Count));
    }

    private string WithAchievementLineType(string line, int index, int count)
    {
        // Unlock status is the entry's value, so speak the control type first.
        int statusAt = line.LastIndexOf(", unlocked", StringComparison.OrdinalIgnoreCase);
        if (statusAt < 0)
            statusAt = line.LastIndexOf(", locked", StringComparison.OrdinalIgnoreCase);
        string typedLine = statusAt < 0 ? WithControlType(line, "list item") :
            WithControlType(line[..statusAt], "list item") + line[statusAt..];
        return WithMenuIndex(typedLine, index, count);
    }

    private static InputAction? GetAchievementMoveAction(BookController controller)
    {
        // The book runs its own PlayerInput map, which may have bindings
        // different from the main menu's EventSystem navigation action.
        var actions = controller.playerInput?.actions;
        InputAction? move = actions?.FindAction("Navigate", false);
        if (move != null && move.enabled)
            return move;
        move = actions?.FindAction("UI/Move", false);
        if (move != null && move.enabled)
            return move;
        move = actions?.FindAction("UI/Navigate", false);
        if (move != null && move.enabled)
            return move;

        EventSystem? eventSystem = EventSystem.current;
        InputSystemUIInputModule? inputModule = eventSystem == null
            ? null : eventSystem.GetComponent<InputSystemUIInputModule>();
        move = inputModule?.move?.action;
        return move != null && move.enabled ? move : null;
    }

    private void SpeakAchievementOpeningIfDelayed()
    {
        if (_achievementOpeningSpoken ||
            Environment.TickCount64 - _achievementOpenedAt < 800)
            return;

        _achievementOpeningSpoken = true;
        QueueFocusSpeech("Achievements book.");
    }

    private static List<string> ReadAchievementPage(BookController controller)
    {
        var lines = new List<string>();
        foreach (PageView view in controller.PageViews)
        {
            if (view == null || !view.gameObject.activeInHierarchy ||
                (view.pageViewCamera != null && !view.pageViewCamera.isActiveAndEnabled))
                continue;

            AchievementListPageView? listPage = view.GetComponent<AchievementListPageView>();
            if (listPage != null)
            {
                AddAchievementListPageLines(listPage, lines);
                continue;
            }

            StickerPageView? stickerPage = view.GetComponent<StickerPageView>();
            if (stickerPage != null)
                AddAchievementStickerPageLines(stickerPage, lines);
        }

        return lines;
    }

    private void AnnounceAchievementBookClosed()
    {
        if (!_achievementBookWasOpen)
            return;

        _achievementBookWasOpen = false;
        _achievementCloseFocusHold = true;
        _achievementCloseHoldUntil = Environment.TickCount64 + 1000;
        _achievementOpeningSpoken = false;
        _lastAchievementSpread = null;
        _lastAchievementPage = null;
        _achievementLines.Clear();
        _achievementLineIndex = 0;
        _achievementMoveDirection = 0;
        QueueSpeech("Achievements book closed.");
        WriteStatus("Achievements book closed after its pages were opened.");
    }

    private bool HoldAchievementCloseUntilFocusMoves()
    {
        if (!_achievementCloseFocusHold)
            return false;

        if (_mainMenu == null)
            _mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();

        Panel? menuPanel = _mainMenu?.mainMenuPanel;
        Button? achievementButton = _mainMenu?.achievementsButton;
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        Button? focusedButton = selected?.GetComponentInParent<Button>();
        if (menuPanel != null && menuPanel.IsVisible && menuPanel.gameObject.activeInHierarchy &&
            achievementButton != null && focusedButton != null &&
            achievementButton.GetInstanceID() == focusedButton.GetInstanceID())
            return true;

        // The EventSystem briefly has no selection while the book retracts.
        // Keep the close announcement intact, but release immediately when
        // another menu button gets focus.
        if (focusedButton == null && Environment.TickCount64 < _achievementCloseHoldUntil)
            return true;

        _achievementCloseFocusHold = false;
        return false;
    }

    private static void AddAchievementListPageLines(AchievementListPageView page, List<string> lines)
    {
        string? group = CleanSpeechValue(page.Group);
        if (group != null)
            lines.Add(group);

        // Walking transforms keeps subgroup headings in the same order as
        // the achievements shown beneath them in the rendered book.
        foreach (Transform child in page.GetComponentsInChildren<Transform>(true))
        {
            AchievementSubgroupItem? subgroup = child.GetComponent<AchievementSubgroupItem>();
            if (subgroup != null)
            {
                string? heading = CleanSpeechValue(subgroup.Description)
                    ?? CleanSpeechValue(subgroup.text?.text);
                if (heading != null)
                    lines.Add(heading);
                continue;
            }

            AchievementListItem? item = child.GetComponent<AchievementListItem>();
            if (item == null)
                continue;

            BopItAchievement? achievement = item.Achievement;
            string? title = CleanSpeechValue(achievement?.Title);
            string? description = CleanSpeechValue(item.Description)
                ?? CleanSpeechValue(item.text?.text)
                ?? CleanSpeechValue(achievement?.Description);
            if (title == null && description == null)
                continue;

            string result = title ?? description!;
            if (title != null && description != null &&
                !string.Equals(title, description, StringComparison.OrdinalIgnoreCase))
                result += $", {description}";
            if (achievement != null)
                result += achievement.IsUnlocked ? ", unlocked" : ", locked";
            lines.Add(result);
        }
    }

    private static void AddAchievementStickerPageLines(StickerPageView page, List<string> lines)
    {
        string? group = CleanSpeechValue(page.Group);
        if (group != null)
            lines.Add(group);

        foreach (BaseSticker sticker in page.GetComponentsInChildren<BaseSticker>(true))
        {
            string? title = CleanSpeechValue(sticker.Title);
            string? description = CleanSpeechValue(sticker.Description);
            if (title == null && description == null)
                continue;

            string result = title ?? description!;
            if (title != null && description != null &&
                !string.Equals(title, description, StringComparison.OrdinalIgnoreCase))
                result += $", {description}";
            result += sticker.IsUnlocked ? ", unlocked" : ", locked";
            lines.Add(result);
        }
    }

    private void ResetAchievementsFocus()
    {
        _achievementBookController = null;
        _achievementBookWasOpen = false;
        _achievementOpeningSpoken = false;
        _lastAchievementSpread = null;
        _lastAchievementPage = null;
        _achievementOpenedAt = 0;
        _nextAchievementContentReadAt = 0;
        _achievementLines.Clear();
        _achievementLineIndex = 0;
        _achievementMoveDirection = 0;
        _nextAchievementMoveAt = 0;
    }
}
