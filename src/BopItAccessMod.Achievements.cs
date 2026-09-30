using Il2Cpp;
using Il2Cppecho17.EndlessBook;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private AchievementsPanel? _achievementsPanel;
    private BookController? _achievementBookController;
    private long _nextAchievementSearchAt;
    private long _nextAchievementBookSearchAt;
    private long _nextAchievementContentReadAt;
    private long _achievementOpenedAt;
    private bool _achievementsWasVisible;
    private bool _achievementOpeningSpoken;
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
            ResetAchievementsFocus();
            long now = Environment.TickCount64;
            if (now < _nextAchievementSearchAt)
                return false;

            _nextAchievementSearchAt = now + 500;
            _achievementsPanel = UnityEngine.Object.FindFirstObjectByType<AchievementsPanel>();
            if (_achievementsPanel == null)
                return false;
        }

        if (!_achievementsPanel.gameObject.activeInHierarchy ||
            _achievementsPanel.achievementViewState == AchievementViewState.Hidden)
        {
            ResetAchievementsFocus();
            return false;
        }

        if (!_achievementsWasVisible)
        {
            _achievementsWasVisible = true;
            _achievementOpenedAt = Environment.TickCount64;
            WriteStatus("Achievements book opened; waiting for its current pages.");
        }

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
        if (book == null || controller!.IsFlipping || book.IsTurningPages)
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

        List<string> pageLines = ReadAchievementPage(controller, book);
        if (pageLines.Count == 0)
        {
            SpeakAchievementOpeningIfDelayed();
            return true;
        }

        string pageKey = $"{book.CurrentLeftPageNumber}:{book.CurrentRightPageNumber}:{string.Join("\u001f", pageLines)}";
        if (!string.Equals(pageKey, _lastAchievementPage, StringComparison.Ordinal))
        {
            bool firstPage = _lastAchievementPage == null;
            _lastAchievementPage = pageKey;
            _achievementLines.Clear();
            _achievementLines.AddRange(pageLines);
            _achievementLineIndex = 0;
            _achievementMoveDirection = 0;
            string introduction = firstPage && !_achievementOpeningSpoken
                ? "Achievements book. " : "Achievements page. ";
            string announcement = introduction + _achievementLines[0];
            if (_achievementLines.Count > 1)
            {
                if (GetAchievementMoveAction(controller) != null)
                    announcement += " Use up and down to read this page.";
                else
                    announcement += " " + string.Join(". ", _achievementLines.Skip(1));
            }
            if (firstPage)
                announcement += " Use left and right arrows or controller shoulder buttons to turn pages. Back to return.";
            QueueSpeech(announcement, firstPage ? !_achievementOpeningSpoken : true);
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
        QueueSpeech(_achievementLines[nextIndex]);
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
        QueueSpeech("Achievements book. Use left and right arrows or controller shoulder buttons to turn pages. Back to return.");
    }

    private static List<string> ReadAchievementPage(BookController controller, EndlessBook book)
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

        if (lines.Count > 0)
            return lines;

        // The book can be on its cover before it is opened. Preserve the
        // game's Bop and Back controls while explaining how to reach pages.
        if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
            lines.Add("Achievements book closed. Bop to open. Back to return.");
        return lines;
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
        _achievementsWasVisible = false;
        _achievementOpeningSpoken = false;
        _lastAchievementPage = null;
        _achievementOpenedAt = 0;
        _nextAchievementContentReadAt = 0;
        _achievementLines.Clear();
        _achievementLineIndex = 0;
        _achievementMoveDirection = 0;
        _nextAchievementMoveAt = 0;
    }
}
