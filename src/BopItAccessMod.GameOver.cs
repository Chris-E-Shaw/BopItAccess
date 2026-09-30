using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private GameUIManager? _gameOverUi;
    private App? _gameOverApp;
    private long _nextGameOverSearchAt;
    private bool _gameOverWasVisible;
    private bool _lastSoloHighScore;
    private int _lastGameOverPanelId;
    private int _lastGameOverFocusedId;
    private string? _lastGameOverPrompts;
    private string? _lastGameOverWinner;
    private int _lastGameOverRank;
    private long _backOnlyPromptDueAt;
    private bool _backOnlyPromptSpoken;
    private long _gameOverScoreSpeechProtectedUntil;
    // The SAPI trim path renders a complete memory stream before playback.
    // Hold focus announcements while that initial score is being dispatched.
    private long _gameOverScoreDispatchPendingUntil;
    private readonly List<string> _deferredGameOverResultUpdates = new();
    private string? _deferredGameOverMenuUpdate;
    private bool _soloBackInstructionPending;
    private bool _gameOverReadScoreInstructionPending;

    // The final score belongs to the kill-screen panel. Its displayed TMP
    // number animates from zero, so read the stored final score instead.
    private bool ReadGameOverFocus()
    {
        if (_gameOverUi == null)
        {
            // The game scene is destroyed on return to the main menu. A new
            // scene must always get a fresh entrance announcement.
            ResetGameOverFocus();
            long now = Environment.TickCount64;
            if (now < _nextGameOverSearchAt)
                return false;

            _nextGameOverSearchAt = now + 500;
            _gameOverUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
            if (_gameOverUi == null)
                return false;
        }

        FinalScorePanel? final = _gameOverUi.finalScorePanel;
        if (final == null || !final.IsVisible || !final.gameObject.activeInHierarchy)
        {
            ResetGameOverFocus();
            return false;
        }

        SoloKillScreenPanel? solo = final.soloKillScreenPanel;
        WithFriendsKillScreenPanel? friends = final.withFriendsKillScreenPanel;
        bool soloVisible = solo != null && solo.IsVisible && solo.gameObject.activeInHierarchy;
        bool friendsVisible = friends != null && friends.IsVisible && friends.gameObject.activeInHierarchy;
        if (!soloVisible && !friendsVisible)
        {
            ResetGameOverFocus();
            return false;
        }

        Panel panel = soloVisible ? solo! : friends!;
        int panelId = panel.GetInstanceID();
        if (panelId != _lastGameOverPanelId)
        {
            ResetGameOverFocus();
            _lastGameOverPanelId = panelId;
        }

        GameMode? mode = null;
        string? initialWinner = null;
        if (friendsVisible)
        {
            mode = ReadGameOverMode();
            if (!mode.HasValue)
            {
                WasReadScorePressed(false);
                return true;
            }

            // One-on-one never assigns the numeric score field on this
            // screen. Wait for the winner selected by FinalScorePanel.Init.
            if (mode == GameMode.OneOnOne)
            {
                initialWinner = ReadOneOnOneWinner(friends!);
                if (initialWinner == null)
                {
                    WasReadScorePressed(false);
                    return true;
                }
            }
        }

        // READ SCORE is enabled only while the actual result UI is visible
        // and the game has finished. A cached score must never be available
        // during play or on the song-selection screen.
        bool readScoreAvailable = _gameOverUi.gameManager != null &&
            _gameOverUi.gameManager.GameState == GameState.GameOver;

        if (!_gameOverWasVisible)
        {
            _gameOverWasVisible = true;
            _gameOverReadScoreInstructionPending = true;
            // Enable the action for this screen, but let the automatic first
            // result announcement have its own turn.
            WasReadScorePressed(readScoreAvailable);
            if (soloVisible)
            {
                string score = $"Score: {solo!.playerScore}.";
                // The panel can retain its previous high-score flag. Only a
                // current result callback makes that flag trustworthy.
                _lastSoloHighScore = GetFreshResultRank() == 1 && solo.isHighScore;
                if (_lastSoloHighScore)
                    score += " New high score.";

                (int focusedId, _) = GetFocusedSoloResultButton(solo);
                _lastGameOverFocusedId = focusedId != 0
                    ? focusedId : solo.replayButton?.GetInstanceID() ?? 0;
                ProtectGameOverScoreSpeech(score);
                _deferredGameOverMenuUpdate = "Replay. Leaderboard. Back to return.";
                _soloBackInstructionPending = true;
                QueueScoreThenMenu(score, null);
                WriteStatus($"Solo game over visible; {score}");
            }
            else
            {
                WithFriendsKillScreenPanel currentFriends = friends!;
                _lastGameOverWinner = initialWinner;
                string score = mode == GameMode.OneOnOne
                    ? $"{_lastGameOverWinner}."
                    : $"Score: {friends!.playerScore}.";

                int rank = ReadFriendsRank(currentFriends, mode!.Value);
                _lastGameOverRank = GetFreshResultRank() == rank ? rank : 0;
                if (_lastGameOverRank > 0)
                    score += $" Rank: {_lastGameOverRank}.";

                _lastGameOverPrompts = ReadFriendsPrompts(currentFriends, mode!.Value);
                string? initialPrompts = _lastGameOverPrompts;
                if (initialPrompts == "Back to return.")
                {
                    // Back is visible during the score animation, before the
                    // actual Continue/Replay prompt appears. Give it time.
                    initialPrompts = null;
                    _backOnlyPromptDueAt = Environment.TickCount64 + 1200;
                }
                ProtectGameOverScoreSpeech(score);
                _deferredGameOverMenuUpdate = initialPrompts;
                QueueScoreThenMenu(score, null);
                WriteStatus($"With-friends game over visible; {score}");
            }

            return true;
        }

        if (soloVisible)
        {
            var resultChanges = new List<string>(1);
            string? menuChange = null;
            if (GetFreshResultRank() == 1 && solo!.isHighScore && !_lastSoloHighScore)
            {
                _lastSoloHighScore = true;
                resultChanges.Add("New high score.");
            }

            (int focusedId, string? label) = GetFocusedSoloResultButton(solo!);
            if (focusedId != 0 && focusedId != _lastGameOverFocusedId)
            {
                _lastGameOverFocusedId = focusedId;
                if (label != null)
                    menuChange = label;
            }

            AnnounceGameOverUpdates(resultChanges, menuChange, "Solo");
        }
        else
        {
            WithFriendsKillScreenPanel currentFriends = friends!;
            var resultChanges = new List<string>(2);
            string? menuChange = null;
            string? prompts = ReadFriendsPrompts(currentFriends, mode!.Value);
            if (prompts != null && !string.Equals(prompts, _lastGameOverPrompts, StringComparison.Ordinal))
            {
                _lastGameOverPrompts = prompts;
                if (prompts == "Back to return.")
                    _backOnlyPromptDueAt = Environment.TickCount64 + 1200;
                else
                {
                    _backOnlyPromptDueAt = 0;
                    menuChange = prompts;
                }
            }

            if (prompts == "Back to return." && !_backOnlyPromptSpoken &&
                _backOnlyPromptDueAt != 0 && Environment.TickCount64 >= _backOnlyPromptDueAt)
            {
                _backOnlyPromptSpoken = true;
                menuChange = prompts;
            }

            string? winner = mode == GameMode.OneOnOne
                ? ReadOneOnOneWinner(currentFriends) : null;
            if (winner != null && !string.Equals(winner, _lastGameOverWinner, StringComparison.Ordinal))
            {
                _lastGameOverWinner = winner;
                resultChanges.Add($"{winner}.");
            }

            int rank = ReadFriendsRank(currentFriends, mode!.Value);
            if (rank > 0 && GetFreshResultRank() == rank && rank != _lastGameOverRank)
            {
                _lastGameOverRank = rank;
                resultChanges.Add($"Rank: {rank}.");
            }

            AnnounceGameOverUpdates(resultChanges, menuChange, "With-friends");
        }

        // Handle a manual repeat after focus updates, so a focus change in
        // the same frame cannot replace the requested result before Tolk
        // receives it. Later focus changes may interrupt it normally.
        if (WasReadScorePressed(readScoreAvailable))
            AnnounceFinalResultOnDemand(soloVisible ? solo : null,
                friendsVisible ? friends : null, mode);

        return true;
    }

    private void AnnounceFinalResultOnDemand(SoloKillScreenPanel? solo,
        WithFriendsKillScreenPanel? friends, GameMode? mode)
    {
        string? result;
        if (solo != null)
        {
            result = $"Score: {solo.playerScore}.";
            if (GetFreshResultRank() == 1 && solo.isHighScore)
                result += " New high score.";
        }
        else if (friends != null && mode.HasValue)
        {
            if (mode == GameMode.OneOnOne)
            {
                string? winner = ReadOneOnOneWinner(friends);
                if (winner == null)
                    return;
                result = $"{winner}.";
            }
            else
                result = $"Score: {friends.playerScore}.";

            int rank = ReadFriendsRank(friends, mode.Value);
            if (rank > 0 && GetFreshResultRank() == rank)
                result += $" Rank: {rank}.";
        }
        else
            return;

        // A requested repeat can be requested again at any time. Speak it
        // immediately and let subsequent menu navigation interrupt it.
        QueueSpeech(result);
        WriteStatus($"Read Score on final result screen: {result}");
    }

    private void ProtectGameOverScoreSpeech(string score)
    {
        // NVDA does not expose speech completion through Tolk. Estimate the
        // time needed for the short score sentence before speaking the menu.
        long now = Environment.TickCount64;
        Volatile.Write(ref _gameOverScoreSpeechProtectedUntil,
            now + EstimateGameOverScoreSpeechMs(score));
        Volatile.Write(ref _gameOverScoreDispatchPendingUntil,
            _speechEnabled ? now + 30000 : 0);
    }

    private static long EstimateGameOverScoreSpeechMs(string score)
    {
        int wordCount = score.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Clamp(1000L + wordCount * 550L, 2200L, 4500L);
    }

    private void CompleteGameOverScoreSpeechDispatch(string score, bool accepted)
    {
        if (accepted)
            ExtendGameOverScoreProtection(EstimateGameOverScoreSpeechMs(score));
        // Publish the deadline before releasing the hold on menu focus.
        Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
    }

    private void ExtendGameOverScoreProtectionForSapiPlayback(int pcmByteLength)
    {
        // The capture format is 22,050 Hz, 16-bit mono. Give playback a
        // short device-start margin after its actual PCM duration.
        long durationMs = Math.Clamp(pcmByteLength * 1000L / (22050 * 2) + 500,
            1500L, 30000L);
        ExtendGameOverScoreProtection(durationMs);
    }

    private void ExtendGameOverScoreProtection(long durationMs)
    {
        long requested = Environment.TickCount64 + durationMs;
        long current;
        do
        {
            current = Volatile.Read(ref _gameOverScoreSpeechProtectedUntil);
            if (current >= requested)
                return;
        }
        while (Interlocked.CompareExchange(
            ref _gameOverScoreSpeechProtectedUntil, requested, current) != current);
    }

    private void AnnounceGameOverUpdates(List<string> resultChanges,
        string? menuChange, string context)
    {
        // Keep score-related updates that arrive during the protection window,
        // but replace earlier menu choices with the most recent focused item.
        _deferredGameOverResultUpdates.AddRange(resultChanges);
        if (menuChange != null)
            _deferredGameOverMenuUpdate = menuChange;

        long now = Environment.TickCount64;
        if (now < Volatile.Read(ref _gameOverScoreDispatchPendingUntil) ||
            now < Volatile.Read(ref _gameOverScoreSpeechProtectedUntil))
            return;

        if (_deferredGameOverResultUpdates.Count == 0 &&
            _deferredGameOverMenuUpdate == null &&
            !_gameOverReadScoreInstructionPending)
            return;

        var changes = new List<string>(_deferredGameOverResultUpdates);
        if (_deferredGameOverMenuUpdate != null)
            changes.Add(_deferredGameOverMenuUpdate);
        if (_soloBackInstructionPending &&
            !(_deferredGameOverMenuUpdate?.Contains("Back to return.",
                StringComparison.OrdinalIgnoreCase) ?? false))
            changes.Add("Back to return.");
        if (_gameOverReadScoreInstructionPending)
            changes.Add(ReadScoreBindingInstruction());
        string announcement = string.Join(" ", changes);
        _deferredGameOverResultUpdates.Clear();
        _deferredGameOverMenuUpdate = null;
        _soloBackInstructionPending = false;
        _gameOverReadScoreInstructionPending = false;

        // The score has had its own protected turn. Later navigation now
        // interrupts stale Replay/Leaderboard announcements as elsewhere.
        QueueSpeech(announcement);
        WriteStatus($"{context} game over update: {announcement}");
    }

    private GameMode? ReadGameOverMode()
    {
        GameManager? manager = _gameOverUi?.gameManager;
        if (manager != null)
            return manager.GameMode;

        if (_gameOverApp == null)
            _gameOverApp = UnityEngine.Object.FindFirstObjectByType<App>();
        return _gameOverApp == null ? null : _gameOverApp.GameMode;
    }

    private static (int Id, string? Label) GetFocusedSoloResultButton(SoloKillScreenPanel panel)
    {
        GameObject? selected = EventSystem.current == null
            ? null : EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(panel.transform))
            return (0, null);

        Button? button = selected.GetComponentInParent<Button>();
        if (button == null || !button.transform.IsChildOf(panel.transform))
            return (0, null);

        if (panel.replayButton != null && button.GetInstanceID() == panel.replayButton.GetInstanceID())
            return (button.GetInstanceID(), "Replay");
        if (panel.leaderboardButton != null && button.GetInstanceID() == panel.leaderboardButton.GetInstanceID())
            return (button.GetInstanceID(), "Leaderboard");
        return (0, null);
    }

    private static string? ReadFriendsPrompts(WithFriendsKillScreenPanel panel, GameMode mode)
    {
        var prompts = new List<string>(3);
        if (IsGameOverPromptVisible(panel.continuePrompt))
            prompts.Add(mode == GameMode.Party ? "Continue to leaderboard" : "Continue");
        if (IsGameOverPromptVisible(panel.replayPrompt))
            prompts.Add("Replay");
        if (IsGameOverPromptVisible(panel.backButton))
            prompts.Add("Back to return");
        return prompts.Count == 0 ? null : string.Join(". ", prompts) + ".";
    }

    private static bool IsGameOverPromptVisible(Panel? prompt) =>
        prompt != null && prompt.IsVisible && prompt.gameObject.activeInHierarchy;

    private static string? ReadOneOnOneWinner(WithFriendsKillScreenPanel panel)
    {
        // These objects are both active in the saved scene. The game toggles
        // their own active state when it knows the one-on-one winner.
        bool green = panel.greenWins != null && panel.greenWins.activeSelf;
        bool yellow = panel.yellowWins != null && panel.yellowWins.activeSelf;
        if (green == yellow)
            return null;
        return green ? "Green wins" : "Yellow wins";
    }

    private static int ReadFriendsRank(WithFriendsKillScreenPanel panel, GameMode mode)
    {
        if (mode != GameMode.Party || !IsGameOverPromptVisible(panel.rankPanel))
            return 0;

        int rank = panel.leaderboardManager?.MostRecentRank ?? 0;
        return rank is >= 1 and <= 3 ? rank : 0;
    }

    private void ResetGameOverFocus()
    {
        WasReadScorePressed(false);
        _gameOverWasVisible = false;
        _lastSoloHighScore = false;
        _lastGameOverPanelId = 0;
        _lastGameOverFocusedId = 0;
        _lastGameOverPrompts = null;
        _lastGameOverWinner = null;
        _lastGameOverRank = 0;
        _backOnlyPromptDueAt = 0;
        _backOnlyPromptSpoken = false;
        Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
        Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
        _deferredGameOverResultUpdates.Clear();
        _deferredGameOverMenuUpdate = null;
        _soloBackInstructionPending = false;
        _gameOverReadScoreInstructionPending = false;
    }
}
