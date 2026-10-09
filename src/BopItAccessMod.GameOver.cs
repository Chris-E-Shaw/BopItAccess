using System.Runtime.Versioning;
using HarmonyLib;
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
    private readonly List<string> _gameOverResultChanges = new(2);
    private string? _deferredGameOverMenuUpdate;
    private long _nextRoundSpeechCancellationErrorAt;

    internal static void NoteRoundSpeechStarting(string reason)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeNativeAudioMod);
        if (mod == null || mod._modStopping)
            return;

        try
        {
            // Observe native Replay and PlayingStart directly: the result
            // panel can hide before the state transition, and Replay skips
            // the song-selection panel that formerly owned cancellation.
            // PlayingStart also runs before the new round's spoken colour.
            mod.StopSpeechForGameStart();
            mod.ResetGameOverFocus();
            mod.ResetTrackSelectFocus();
            mod._trackSelectStartTransitionUntil = 0;
            mod._buttonHintContextKey = null;
            mod._cachedButtonHintContext = null;
            mod._nextButtonHintContextProbeAt = 0;
            mod.ResetButtonHintTimers(Environment.TickCount64);
            WriteStatus(reason +
                "; cleared and interrupted all menu and result speech.");
        }
        catch (Exception ex)
        {
            // Optional speech observation must never block Replay or native
            // gameplay, even if a Unity object disappears during transition.
            long now = Environment.TickCount64;
            if (now >= mod._nextRoundSpeechCancellationErrorAt)
            {
                mod._nextRoundSpeechCancellationErrorAt = now + 5000;
                WriteStatus("Round-start speech cancellation failed: " + ex.Message);
            }
        }
    }

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

        // Native hide animations can leave the old result panel visible in
        // the first replay frame. Do not queue that old score after the
        // round-start hook has just cancelled it.
        GameManager? game = _gameOverUi.gameManager;
        if (game == null || game.GameState != GameState.GameOver)
        {
            ResetGameOverFocus();
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
            // Enable the action for this screen, but let the automatic first
            // result announcement have its own turn.
            WasReadScorePressed(readScoreAvailable);
            if (soloVisible)
            {
                string score = LF("Score: {0}.", solo!.playerScore);
                // The panel can retain its previous high-score flag. Only a
                // current result callback makes that flag trustworthy.
                _lastSoloHighScore = GetFreshResultRank() == 1 && solo.isHighScore;
                if (_lastSoloHighScore)
                    score += " " + L("New high score.");

                (int focusedId, _) = GetFocusedSoloResultButton(solo);
                _lastGameOverFocusedId = focusedId != 0
                    ? focusedId : solo.replayButton?.GetInstanceID() ?? 0;
                ProtectGameOverScoreSpeech(score);
                _deferredGameOverMenuUpdate = ReadSoloResultPrompts(solo);
                QueueScoreThenMenu(score, null);
                WriteStatus($"Solo game over visible; {score}");
            }
            else
            {
                WithFriendsKillScreenPanel currentFriends = friends!;
                _lastGameOverWinner = initialWinner;
                string score = mode == GameMode.OneOnOne
                    ? L(_lastGameOverWinner!) + "."
                    : LF("Score: {0}.", friends!.playerScore);

                int rank = ReadFriendsRank(currentFriends, mode!.Value);
                _lastGameOverRank = GetFreshResultRank() == rank ? rank : 0;
                if (_lastGameOverRank > 0)
                    score += " " + LF("Rank: {0}.", _lastGameOverRank);

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
            _gameOverResultChanges.Clear();
            string? menuChange = null;
            if (GetFreshResultRank() == 1 && solo!.isHighScore && !_lastSoloHighScore)
            {
                _lastSoloHighScore = true;
                _gameOverResultChanges.Add(L("New high score."));
            }

            (int focusedId, string? label) = GetFocusedSoloResultButton(solo!);
            if (focusedId != 0 && focusedId != _lastGameOverFocusedId)
            {
                _lastGameOverFocusedId = focusedId;
                if (label != null)
                    menuChange = label;
            }

            AnnounceGameOverUpdates(_gameOverResultChanges, menuChange, "Solo");
        }
        else
        {
            WithFriendsKillScreenPanel currentFriends = friends!;
            _gameOverResultChanges.Clear();
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
                _gameOverResultChanges.Add(L(winner) + ".");
            }

            int rank = ReadFriendsRank(currentFriends, mode!.Value);
            if (rank > 0 && GetFreshResultRank() == rank && rank != _lastGameOverRank)
            {
                _lastGameOverRank = rank;
                _gameOverResultChanges.Add(LF("Rank: {0}.", rank));
            }

            AnnounceGameOverUpdates(_gameOverResultChanges, menuChange, "With-friends");
        }

        // Handle a manual repeat after focus updates, so a focus change in
        // the same frame cannot replace the requested result before Prism
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
            result = LF("Score: {0}.", solo.playerScore);
            if (GetFreshResultRank() == 1 && solo.isHighScore)
                result += " " + L("New high score.");
        }
        else if (friends != null && mode.HasValue)
        {
            if (mode == GameMode.OneOnOne)
            {
                string? winner = ReadOneOnOneWinner(friends);
                if (winner == null)
                    return;
                result = L(winner) + ".";
            }
            else
                result = LF("Score: {0}.", friends.playerScore);

            int rank = ReadFriendsRank(friends, mode.Value);
            if (rank > 0 && GetFreshResultRank() == rank)
                result += " " + LF("Rank: {0}.", rank);
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
        // Prism does not expose completion for every backend. Estimate the
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
        long estimate = 1000L + wordCount * 550L;
        // Japanese and Chinese normally have no word spaces, and Korean score
        // phrases can be compact too. Give the entire translated result time
        // to finish before the focused Replay/Leaderboard choice may speak.
        if (CurrentGameLocale is "ja" or "ko" or "zh")
        {
            int spokenCharacters = score.Count(char.IsLetterOrDigit);
            estimate = Math.Max(estimate, 1100L + spokenCharacters * 210L);
        }
        return Math.Clamp(estimate, 2200L, 6000L);
    }

    private void CompleteGameOverScoreSpeechDispatch(string score, bool accepted,
        long generation)
    {
        lock (_speechLock)
        {
            // Replay may cancel a score while its output driver is returning.
            // That old dispatch must not restore a hold in the new round.
            if (generation != _speechGeneration)
                return;
            if (_speechSuppressedForBackground)
            {
                Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
                Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
                return;
            }
            if (!accepted)
            {
                // A newer score can have replaced this batch. Its pending
                // deadline belongs to that newer request.
                if (_pendingPrioritySpeech == null)
                {
                    Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
                    Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
                }
                return;
            }
            ExtendGameOverScoreProtection(EstimateGameOverScoreSpeechMs(score));
            // Publish the deadline before releasing the hold on menu focus.
            if (_pendingPrioritySpeech == null)
                Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
        }
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
            _deferredGameOverMenuUpdate == null)
            return;

        var changes = new List<string>(_deferredGameOverResultUpdates);
        // The Back prompt is a menu choice; its "to return" instruction and
        // READ SCORE binding belong to the shared button hint for this screen.
        string? menu = _deferredGameOverMenuUpdate?.Replace("Back to return.",
            "Back.", StringComparison.OrdinalIgnoreCase)?.Trim();
        bool hasMenuAnnouncement = !string.IsNullOrEmpty(menu);
        if (hasMenuAnnouncement)
            changes.Add(LocalizeResultPromptText(menu!));
        string announcement = string.Join(" ", changes);
        _deferredGameOverResultUpdates.Clear();
        _deferredGameOverMenuUpdate = null;
        if (announcement.Length == 0)
            return;

        // The score has had its own protected turn. Later navigation now
        // interrupts stale Replay/Leaderboard announcements as elsewhere.
        if (hasMenuAnnouncement)
            QueueFocusSpeech(announcement);
        else
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

    private (int Id, string? Label) GetFocusedSoloResultButton(SoloKillScreenPanel panel)
    {
        GameObject? selected = EventSystem.current == null
            ? null : EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(panel.transform))
            return (0, null);

        Button? button = selected.GetComponentInParent<Button>();
        if (button == null || !button.transform.IsChildOf(panel.transform))
            return (0, null);

        bool replayAvailable = IsAvailableSoloResultButton(panel, panel.replayButton);
        bool leaderboardAvailable = IsAvailableSoloResultButton(panel, panel.leaderboardButton);
        int count = (replayAvailable ? 1 : 0) + (leaderboardAvailable ? 1 : 0);
        if (replayAvailable && button.GetInstanceID() == panel.replayButton!.GetInstanceID())
            return (button.GetInstanceID(),
                WithMenuIndex(WithControlType(L("Replay"), "button"), 0, count));
        if (leaderboardAvailable &&
            button.GetInstanceID() == panel.leaderboardButton!.GetInstanceID())
            return (button.GetInstanceID(),
                WithMenuIndex(WithControlType(L("Leaderboard"), "button"),
                    replayAvailable ? 1 : 0, count));
        return (0, null);
    }

    private string ReadSoloResultPrompts(SoloKillScreenPanel panel)
    {
        // Result buttons can be non-interactable during the score animation.
        // They are still the two choices that appear when the menu opens.
        return WithMenuIndex(WithControlType(L("Replay"), "button"), 0, 2) + ". " +
            WithMenuIndex(WithControlType(L("Leaderboard"), "button"), 1, 2) +
            ". " + L("Back") + ".";
    }

    private static bool IsAvailableSoloResultButton(SoloKillScreenPanel panel, Button? button) =>
        button != null && button.gameObject.activeInHierarchy && button.interactable &&
        button.transform.IsChildOf(panel.transform);

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

    // Keep the English prompt list stable for visibility and timing checks;
    // translate its individual instructions only when they are spoken.
    private static string LocalizeResultPromptText(string text)
    {
        string[] pieces = text.Split(". ", StringSplitOptions.None);
        for (int index = 0; index < pieces.Length; index++)
        {
            string punctuation = pieces[index].EndsWith('.') ? "." : "";
            string instruction = pieces[index].TrimEnd('.');
            pieces[index] = instruction switch
            {
                "Continue to leaderboard" or "Continue" or "Replay" or
                "Back to return" or "Back" => L(instruction) + punctuation,
                _ => pieces[index]
            };
        }
        return string.Join(". ", pieces);
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
        return green ? L("Green wins") : L("Yellow wins");
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
    }
}

[HarmonyPatch(typeof(GameUIManager), "Replay")]
[SupportedOSPlatform("windows")]
internal static class ReplaySpeechCancellationPatch
{
    [HarmonyPrefix]
    private static void BeforeReplay() =>
        BopItAccessMod.NoteRoundSpeechStarting("Replay selected");
}

[HarmonyPatch(typeof(GameManager), "PlayingStart")]
[SupportedOSPlatform("windows")]
internal static class PlayingStartSpeechCancellationPatch
{
    [HarmonyPrefix]
    private static void BeforePlayingStart() =>
        BopItAccessMod.NoteRoundSpeechStarting("Gameplay started");
}
