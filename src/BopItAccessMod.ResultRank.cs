using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private readonly object _resultRankLock = new();
    private GameUIManager? _resultRankUi;
    private LeaderboardManager? _resultRankManager;
    private Action<int>? _managedResultRankCallback;
    private Il2CppSystem.Action<int>? _nativeResultRankCallback;
    private long _nextResultRankSearchAt;
    private long _nextResultRankManagerSearchAt;
    private long _nextResultRankErrorAt;
    private long _resultRankGeneration;
    private long _resultRankBaseline;
    private int _resultRankValue;

    // Called throughout gameplay, before the result reader. Listening before
    // GameOverStart catches even a leaderboard callback in the entry frame.
    private void ObserveResultRank()
    {
        try
        {
            long now = Environment.TickCount64;
            if (_resultRankUi == null)
            {
                DetachResultRankListener();
                if (now < _nextResultRankSearchAt)
                    return;

                _nextResultRankSearchAt = now + 500;
                _resultRankUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
                if (_resultRankUi == null)
                    return;
            }

            if (_resultRankManager == null)
            {
                if (now < _nextResultRankManagerSearchAt)
                    return;

                _nextResultRankManagerSearchAt = now + 500;
                LeaderboardManager? manager = UnityEngine.Object.FindFirstObjectByType<LeaderboardManager>();
                if (manager != null)
                    AttachResultRankListener(manager);
            }

            GameManager? game = _resultRankUi.gameManager;
            if (game != null && game.GameState != GameState.GameOver)
            {
                lock (_resultRankLock)
                    _resultRankBaseline = _resultRankGeneration;
            }
        }
        catch (Exception ex)
        {
            if (Environment.TickCount64 < _nextResultRankErrorAt)
                return;

            _nextResultRankErrorAt = Environment.TickCount64 + 5000;
            WriteStatus($"Result rank listener failed: {ex}");
        }
    }

    // A prior result's rank is excluded even when the numeric rank repeats.
    private int? GetFreshResultRank()
    {
        lock (_resultRankLock)
            return _resultRankGeneration > _resultRankBaseline ? _resultRankValue : null;
    }

    private void AttachResultRankListener(LeaderboardManager manager)
    {
        DetachResultRankListener();

        _managedResultRankCallback = OnResultRankUpdated;
        _nativeResultRankCallback =
            DelegateSupport.ConvertDelegate<Il2CppSystem.Action<int>>(_managedResultRankCallback);
        lock (_resultRankLock)
        {
            _resultRankGeneration = 0;
            _resultRankBaseline = 0;
            _resultRankValue = 0;
        }

        manager.MostRecentRankUpdated += _nativeResultRankCallback;
        _resultRankManager = manager;

        WriteStatus("Listening for fresh leaderboard rank updates.");
    }

    private void OnResultRankUpdated(int rank)
    {
        lock (_resultRankLock)
        {
            _resultRankValue = rank;
            _resultRankGeneration++;
        }
    }

    private void DetachResultRankListener()
    {
        LeaderboardManager? manager = _resultRankManager;
        Il2CppSystem.Action<int>? callback = _nativeResultRankCallback;
        _resultRankManager = null;
        _nativeResultRankCallback = null;
        _managedResultRankCallback = null;

        if (manager != null && callback != null)
        {
            try
            {
                manager.MostRecentRankUpdated -= callback;
            }
            catch (Exception ex)
            {
                WriteStatus($"Result rank listener removal failed: {ex.Message}");
            }
        }

        lock (_resultRankLock)
        {
            _resultRankGeneration = 0;
            _resultRankBaseline = 0;
            _resultRankValue = 0;
        }
    }
}
