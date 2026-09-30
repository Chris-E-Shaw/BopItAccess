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
    private MainMenuUIManager? _leaderboardMainMenu;
    private GameUIManager? _leaderboardGameUi;
    private long _nextLeaderboardSearchAt;
    private int _leaderboardPanelId;
    private string? _leaderboardContext;
    private string? _leaderboardRowsSignature;
    private List<string> _leaderboardCachedRows = new();
    private long _nextLeaderboardRowsPollAt;
    private int _leaderboardRowIndex;
    private int _leaderboardSelectedId;
    private long _leaderboardRowsDueAt;
    private bool _leaderboardRowsSpoken;
    private bool _leaderboardEmptySpoken;
    private bool _leaderboardWasLoading;
    private int _leaderboardMoveDirection;
    private long _nextLeaderboardMoveAt;
    private long _leaderboardSuppressUiMoveUntilAt;
    private string? _leaderboardPartyName;
    private PartyLeaderboardState? _leaderboardPartyState;
    private bool _leaderboardContinueVisible;
    private long _leaderboardOuterVisibleAt;

    // CombinedLeaderboardPanel serves both the main menu and the Solo result.
    // PartyLeaderboardPanel is the separate result screen with name selection.
    // Return true during loading too, so the underlying menu cannot speak.
    private bool ReadLeaderboardsFocus()
    {
        long now = Environment.TickCount64;
        if (now >= _nextLeaderboardSearchAt &&
            (_leaderboardMainMenu == null || _leaderboardGameUi == null))
        {
            _nextLeaderboardSearchAt = now + 500;
            if (_leaderboardMainMenu == null)
                _leaderboardMainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_leaderboardGameUi == null)
                _leaderboardGameUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
        }

        PartyLeaderboardPanel? party = _leaderboardGameUi?.partyLeaderboardPanel;
        if (IsLeaderboardPanelVisible(party?.leaderboardWrapper))
            return ReadPartyLeaderboard(party!);

        CombinedLeaderboardPanel? gameCombined = _leaderboardGameUi?.combinedLeaderboardPanel;
        if (IsLeaderboardPanelVisible(gameCombined?.leaderboardWrapper))
            return ReadCombinedLeaderboard(gameCombined!);

        Panel? mainOuter = _leaderboardMainMenu?.leaderboardPanel;
        CombinedLeaderboardPanel? mainCombined =
            mainOuter?.GetComponentInChildren<CombinedLeaderboardPanel>(true);
        if (IsLeaderboardPanelVisible(mainOuter))
        {
            if (mainCombined != null &&
                (mainCombined.leaderboardWrapper == null ||
                 IsLeaderboardPanelVisible(mainCombined.leaderboardWrapper)))
                return ReadCombinedLeaderboard(mainCombined);

            if (_leaderboardOuterVisibleAt == 0)
                _leaderboardOuterVisibleAt = now;
            if (now - _leaderboardOuterVisibleAt >= 500 && _leaderboardPanelId == 0)
            {
                _leaderboardPanelId = mainOuter!.GetInstanceID();
                QueueSpeech("Leaderboards loading.");
            }
            return true;
        }

        ResetLeaderboardsFocus();
        return false;
    }

    private static bool IsLeaderboardPanelVisible(Panel? panel) =>
        panel != null && panel.gameObject.activeInHierarchy &&
        (panel.IsVisible || panel.IsTransitioningShow || panel.IsTransitioningHide);

    private bool ReadCombinedLeaderboard(CombinedLeaderboardPanel panel)
    {
        int panelId = panel.GetInstanceID();
        if (panelId != _leaderboardPanelId)
        {
            ResetLeaderboardsFocus();
            _leaderboardPanelId = panelId;
        }

        long now = Environment.TickCount64;
        string context = ReadCombinedLeaderboardContext(panel);
        if (now >= _nextLeaderboardRowsPollAt || _leaderboardContext == null)
        {
            _leaderboardCachedRows = ReadCombinedLeaderboardRows(panel);
            _nextLeaderboardRowsPollAt = now + 100;
        }
        List<string> rows = _leaderboardCachedRows;
        string rowSignature = string.Join("\n", rows);
        bool loading = IsLeaderboardLoading(panel.loader);
        bool contextChanged = !string.Equals(context, _leaderboardContext, StringComparison.Ordinal);
        bool rowsChanged = !string.Equals(rowSignature, _leaderboardRowsSignature, StringComparison.Ordinal);
        var announcements = new List<string>(3);
        if (contextChanged)
        {
            _leaderboardContext = context;
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 400;
            _leaderboardSelectedId = ReadLeaderboardFocusedItem(panel.transform).Id;
            _leaderboardMoveDirection = 0;
            _leaderboardSuppressUiMoveUntilAt = now + 500;
            announcements.Add($"{context}. {ReadCombinedLeaderboardControls(panel)}" +
                (loading ? " Loading scores." : string.Empty));
            WriteStatus($"Leaderboard context: {context}.");
        }
        else if (rowsChanged)
        {
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 150;
            _leaderboardSuppressUiMoveUntilAt = now + 200;
        }

        if (loading && !_leaderboardWasLoading && !contextChanged)
            announcements.Add("Loading scores.");
        _leaderboardWasLoading = loading;
        string? rowAnnouncement = ReadLeaderboardRowsAfterLoad(rows,
            loading, panel.leaderboardEntryContainerPanel, now);
        if (rowAnnouncement != null)
            announcements.Add(rowAnnouncement);

        (int focusedId, string? focusedText) = ReadLeaderboardFocusedItem(panel.transform);
        if (focusedId != _leaderboardSelectedId)
        {
            _leaderboardSelectedId = focusedId;
            if (focusedText != null)
            {
                if (!contextChanged)
                    announcements.Add(focusedText);
                WriteStatus($"Leaderboard focus: {focusedText}.");
            }
        }

        if (!contextChanged && announcements.Count == 0)
        {
            string? movedRow = ReadLeaderboardVirtualMove(rows, focusedText == null);
            if (movedRow != null)
                announcements.Add(movedRow);
        }
        if (announcements.Count > 0)
            QueueSpeech(string.Join(" ", announcements), contextChanged || rowAnnouncement == null);
        return true;
    }

    private bool ReadPartyLeaderboard(PartyLeaderboardPanel panel)
    {
        int panelId = panel.GetInstanceID();
        if (panelId != _leaderboardPanelId)
        {
            ResetLeaderboardsFocus();
            _leaderboardPanelId = panelId;
        }

        long now = Environment.TickCount64;
        string context = ReadPartyLeaderboardContext(panel);
        if (now >= _nextLeaderboardRowsPollAt || _leaderboardContext == null)
        {
            _leaderboardCachedRows = ReadPartyLeaderboardRows(panel);
            _nextLeaderboardRowsPollAt = now + 100;
        }
        List<string> rows = _leaderboardCachedRows;
        string rowSignature = string.Join("\n", rows);
        bool loading = IsLeaderboardLoading(panel.loader);
        bool contextChanged = !string.Equals(context, _leaderboardContext, StringComparison.Ordinal);
        bool rowsChanged = !string.Equals(rowSignature, _leaderboardRowsSignature, StringComparison.Ordinal);
        var announcements = new List<string>(5);
        if (contextChanged)
        {
            _leaderboardContext = context;
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 400;
            _leaderboardSelectedId = ReadLeaderboardFocusedItem(panel.transform).Id;
            _leaderboardMoveDirection = 0;
            _leaderboardSuppressUiMoveUntilAt = now + 500;
            _leaderboardPartyState = panel.State;
            _leaderboardPartyName = ReadPartySelectedName(panel);
            _leaderboardContinueVisible = IsLeaderboardPanelVisible(panel.ContinuePrompt);
            string initialName = _leaderboardPartyName == null ? string.Empty : $" Selected name: {_leaderboardPartyName}.";
            announcements.Add($"{context}. Choose a name for your score, then continue. Back to return.{initialName} Use Page Up and Page Down to read score rows." +
                (loading ? " Loading scores." : string.Empty));
            WriteStatus($"Party leaderboard context: {context}.");
        }
        else if (rowsChanged)
        {
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 150;
            _leaderboardSuppressUiMoveUntilAt = now + 200;
        }

        if (loading && !_leaderboardWasLoading && !contextChanged)
            announcements.Add("Loading scores.");
        _leaderboardWasLoading = loading;
        string? rowAnnouncement = ReadLeaderboardRowsAfterLoad(rows,
            loading, panel.leaderboardEntryContainerPanel, now);
        if (rowAnnouncement != null)
            announcements.Add(rowAnnouncement);

        string? name = ReadPartySelectedName(panel);
        PartyLeaderboardState state = panel.State;
        bool continueVisible = IsLeaderboardPanelVisible(panel.ContinuePrompt);
        if (state != _leaderboardPartyState)
        {
            _leaderboardPartyState = state;
            _leaderboardPartyName = name;
            string stateSpeech = state switch
            {
                PartyLeaderboardState.Input => name == null ? "Enter a name." : $"Enter a name. {name}.",
                PartyLeaderboardState.Complete => "Name confirmed. Continue.",
                _ => name == null ? "Choose a name." : $"Choose a name. {name}."
            };
            if (!contextChanged)
                announcements.Add(stateSpeech);
            WriteStatus($"Party leaderboard state: {stateSpeech}");
        }
        else if (name != null && !string.Equals(name, _leaderboardPartyName, StringComparison.Ordinal))
        {
            _leaderboardPartyName = name;
            if (!contextChanged)
                announcements.Add($"Name: {name}.");
        }

        if (continueVisible && !_leaderboardContinueVisible)
            announcements.Add("Continue.");
        _leaderboardContinueVisible = continueVisible;

        (int focusedId, string? focusedText) = ReadLeaderboardFocusedItem(panel.transform);
        if (focusedId != _leaderboardSelectedId)
        {
            _leaderboardSelectedId = focusedId;
            if (focusedText != null && !contextChanged)
                announcements.Add(focusedText);
        }

        if (!contextChanged && announcements.Count == 0 && state != PartyLeaderboardState.Input)
        {
            string? movedRow = ReadLeaderboardVirtualMove(rows, focusedText == null);
            if (movedRow != null)
                announcements.Add(movedRow);
        }
        if (announcements.Count > 0)
            QueueSpeech(string.Join(" ", announcements), contextChanged || rowAnnouncement == null);
        return true;
    }

    private static string ReadCombinedLeaderboardContext(CombinedLeaderboardPanel panel)
    {
        string location = panel.Mode == CombinedLeaderboardMode.Game
            ? "Solo leaderboard" : "Leaderboards";
        string? track = ReadLeaderboardTrack(panel.TitleImage, panel.LeaderboardManager, panel.app);
        string? device = ReadLeaderboardDevice(panel.deviceContainer, panel.LeaderboardManager);
        string group = ReadLeaderboardGroup(panel);
        string date = ReadLeaderboardDate(panel);
        string offline = panel.LeaderboardManager != null &&
            !panel.LeaderboardManager.isOnline ? "offline" : string.Empty;
        return string.Join(", ", new[] { location, track, device, group, date, offline }
            .Where(part => !string.IsNullOrEmpty(part)));
    }

    private static string ReadPartyLeaderboardContext(PartyLeaderboardPanel panel)
    {
        LeaderboardManager? manager = panel.LeaderboardManager;
        string? track = ReadLeaderboardTrack(panel.TitleImage, manager, manager?.app);
        string? device = ReadLeaderboardDevice(manager?.DeviceContainer, manager);
        return string.Join(", ", new[] { "Party leaderboard", track, device }
            .Where(part => !string.IsNullOrEmpty(part)));
    }

    private static string? ReadLeaderboardTrack(Image? title, LeaderboardManager? manager, App? app)
    {
        // The leaderboard's title sprite and fetch key belong to the active
        // filter. App.CurrentMusicTrack can lag while the panel changes songs.
        string? sprite = CleanSpeechValue(title?.sprite?.name);
        string? label = NormalizeLeaderboardTrack(sprite);
        if (label != null) return label;
        string? raw = CleanSpeechValue(manager?.trackName);
        label = NormalizeLeaderboardTrack(raw);
        return label ?? ReadTrackSelectTheme(app);
    }

    private static string? NormalizeLeaderboardTrack(string? raw)
    {
        if (raw == null) return null;
        if (raw.Contains("Shapes", StringComparison.OrdinalIgnoreCase)) return "Shapes";
        if (raw.Contains("Space", StringComparison.OrdinalIgnoreCase)) return "Space";
        if (raw.Contains("City", StringComparison.OrdinalIgnoreCase)) return "City";
        if (raw.Contains("Office", StringComparison.OrdinalIgnoreCase)) return "Office";
        // Sprite names often contain layout or resolution suffixes, so only
        // expose an unknown name when it came from the leaderboard fetch key.
        return null;
    }

    private static string? ReadLeaderboardDevice(DeviceContainer? container, LeaderboardManager? manager)
    {
        Device? device = container?.CurrentDevice ?? manager?.currentDevice;
        if (device != null)
            return device.Type == Il2CppBopIt.DeviceType.Extreme ? "Extreme" : "Classic";
        return CleanSpeechValue(manager?.device);
    }

    private static string ReadLeaderboardGroup(CombinedLeaderboardPanel panel)
    {
        if (panel.local != null && panel.local.IsActive) return "Local";
        if (panel.friends != null && panel.friends.IsActive) return "Friends";
        if (panel.global != null && panel.global.IsActive) return "Global";
        GroupFilter? group = panel.LeaderboardManager?.Filters?.Group;
        return group switch
        {
            GroupFilter.Friends => "Friends",
            GroupFilter.Global => "Global",
            _ => "Local"
        };
    }

    private static string ReadLeaderboardDate(CombinedLeaderboardPanel panel)
    {
        if (panel.today != null && panel.today.IsActive) return "Today";
        if (panel.month != null && panel.month.IsActive) return "This month";
        if (panel.allTime != null && panel.allTime.IsActive) return "All time";
        DateRangeFilter? date = panel.LeaderboardManager?.Filters?.DateRange;
        return date switch
        {
            DateRangeFilter.Today => "Today",
            DateRangeFilter.Month => "This month",
            _ => "All time"
        };
    }

    private static string ReadCombinedLeaderboardControls(CombinedLeaderboardPanel panel) =>
        panel.Mode == CombinedLeaderboardMode.Game
            ? "Change song, device, group, or date with the game's controls. Continue or back to return. Use Page Up and Page Down to read score rows."
            : "Change song, device, group, or date with the game's controls. Back to return. Use Page Up and Page Down to read score rows.";

    private static List<string> ReadCombinedLeaderboardRows(CombinedLeaderboardPanel panel)
    {
        var rows = new List<string>();
        var entries = panel.Entries;
        if (entries == null) return rows;
        for (int index = 0; index < entries.Count; index++)
        {
            GameObject? row = entries[index];
            LeaderboardLineItem? entry = row?.GetComponent<LeaderboardLineItem>() ??
                row?.GetComponentInChildren<LeaderboardLineItem>(true);
            if (entry == null || !entry.gameObject.activeInHierarchy) continue;
            BopItScore? score = entry.Score;
            if (score == null) continue;
            string rank = entry.Rank > 0 ? entry.Rank.ToString() :
                CleanSpeechValue(entry.RankText?.text) ?? (index + 1).ToString();
            string alias = CleanSpeechValue(entry.AliasText?.text) ??
                CleanSpeechValue(score?.Identity?.Alias) ?? "Unnamed player";
            string value = score == null
                ? CleanSpeechValue(entry.ScoreText?.text) ?? "unknown"
                : score.Score.ToString();
            string owner = entry.ItemType switch
            {
                LeaderboardItemType.Me => "your score, ",
                LeaderboardItemType.MostRecent => "most recent score, ",
                _ => string.Empty
            };
            rows.Add($"Rank {rank}, {owner}{alias}, score {value}");
        }
        return rows;
    }

    private static List<string> ReadPartyLeaderboardRows(PartyLeaderboardPanel panel)
    {
        var rows = new List<string>();
        var entries = panel.Entries;
        if (entries == null) return rows;
        for (int index = 0; index < entries.Count; index++)
        {
            GameObject? row = entries[index];
            PartyLeaderboardLineItem? entry = row?.GetComponent<PartyLeaderboardLineItem>() ??
                row?.GetComponentInChildren<PartyLeaderboardLineItem>(true);
            if (entry == null || !entry.gameObject.activeInHierarchy) continue;
            BopItScore? score = entry.Score;
            if (score == null) continue;
            string rank = entry.Rank > 0 ? entry.Rank.ToString() :
                CleanSpeechValue(entry.RankText?.text) ?? (index + 1).ToString();
            string alias = CleanSpeechValue(entry.AliasText?.text) ??
                CleanSpeechValue(score?.Identity?.Alias) ?? "Unnamed player";
            string value = score == null
                ? CleanSpeechValue(entry.ScoreText?.text) ?? "unknown"
                : score.Score.ToString();
            string current = entry.ItemType == PartyLeaderboardItemType.Current ? "current score, " : string.Empty;
            rows.Add($"Rank {rank}, {current}{alias}, score {value}");
        }
        return rows;
    }

    private string? ReadLeaderboardRowsAfterLoad(List<string> rows, bool loading,
        Panel? entryContainerPanel, long now)
    {
        if (_leaderboardRowsSpoken || now < _leaderboardRowsDueAt || loading)
            return null;
        if (rows.Count > 0 && entryContainerPanel != null &&
            !IsLeaderboardPanelVisible(entryContainerPanel))
            return null;

        if (rows.Count > 0)
        {
            _leaderboardRowsSpoken = true;
            _leaderboardEmptySpoken = false;
            _leaderboardRowIndex = 0;
            string announcement = $"{rows.Count} scores. {rows[0]}.";
            WriteStatus($"Leaderboard rows loaded: {rows.Count}; first row: {rows[0]}.");
            return announcement;
        }
        else if (!_leaderboardEmptySpoken && now >= _leaderboardRowsDueAt + 900)
        {
            _leaderboardEmptySpoken = true;
            WriteStatus("Leaderboard has no displayed scores.");
            return "No scores available.";
        }
        return null;
    }

    private static bool IsLeaderboardLoading(WaitingSpinner? loader)
    {
        Image? image = loader?.spinner;
        return image != null && image.enabled && image.gameObject.activeInHierarchy &&
            image.color.a > 0.1f;
    }

    private static string? ReadPartySelectedName(PartyLeaderboardPanel panel)
    {
        PartyLeaderboardNameSelect? select = panel.nameSelectEntry;
        if (select == null) return null;
        return CleanSpeechValue(select.NameInput?.text) ??
            CleanSpeechValue(select.playerAlias) ??
            CleanSpeechValue(select.Label?.text);
    }

    private static (int Id, string? Label) ReadLeaderboardFocusedItem(Transform panel)
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(panel))
            return (0, null);

        LeaderboardLineItem? regular = selected.GetComponentInParent<LeaderboardLineItem>();
        if (regular != null)
        {
            string label = $"Rank {regular.Rank}, " +
                $"{CleanSpeechValue(regular.AliasText?.text) ?? "Unnamed player"}, " +
                $"score {regular.Score?.Score.ToString() ?? CleanSpeechValue(regular.ScoreText?.text) ?? "unknown"}";
            return (regular.GetInstanceID(), label);
        }

        PartyLeaderboardNameSelect? nameSelect =
            selected.GetComponentInParent<PartyLeaderboardNameSelect>();
        if (nameSelect != null)
        {
            string? name = CleanSpeechValue(nameSelect.NameInput?.text) ??
                CleanSpeechValue(nameSelect.playerAlias) ??
                CleanSpeechValue(nameSelect.Label?.text);
            return (nameSelect.GetInstanceID(), name == null ? "Choose name" : $"Name: {name}");
        }

        PartyLeaderboardLineItem? party = selected.GetComponentInParent<PartyLeaderboardLineItem>();
        if (party != null)
        {
            string label = $"Rank {party.Rank}, " +
                $"{CleanSpeechValue(party.AliasText?.text) ?? "Unnamed player"}, " +
                $"score {party.Score?.Score.ToString() ?? CleanSpeechValue(party.ScoreText?.text) ?? "unknown"}";
            return (party.GetInstanceID(), label);
        }

        GroupFilterTab? group = selected.GetComponentInParent<GroupFilterTab>();
        if (group != null)
        {
            string? label = CleanSpeechValue(group.Text?.text) ??
                DescribeLeaderboardControlName(group.name);
            return (group.GetInstanceID(), label);
        }

        DateFilterTab? date = selected.GetComponentInParent<DateFilterTab>();
        if (date != null)
            return (date.GetInstanceID(), DescribeLeaderboardControlName(date.name));

        Selectable? control = selected.GetComponentInParent<Selectable>();
        if (control == null || !control.transform.IsChildOf(panel))
            return (selected.GetInstanceID(),
                CleanSpeechValue(selected.GetComponentInChildren<TMP_Text>(true)?.text) ??
                DescribeLeaderboardControlName(selected.name));

        string? labelText = CleanSpeechValue(control.GetComponentInChildren<TMP_Text>(true)?.text);
        labelText ??= DescribeLeaderboardControlName(control.name);
        return (control.GetInstanceID(), labelText);
    }

    private static string? DescribeLeaderboardControlName(string name)
    {
        if (name.Contains("Back", StringComparison.OrdinalIgnoreCase)) return "Back";
        if (name.Contains("Continue", StringComparison.OrdinalIgnoreCase)) return "Continue";
        if (name.Contains("Local", StringComparison.OrdinalIgnoreCase)) return "Local";
        if (name.Contains("Friend", StringComparison.OrdinalIgnoreCase)) return "Friends";
        if (name.Contains("Global", StringComparison.OrdinalIgnoreCase)) return "Global";
        if (name.Contains("Today", StringComparison.OrdinalIgnoreCase)) return "Today";
        if (name.Contains("Month", StringComparison.OrdinalIgnoreCase)) return "This month";
        if (name.Contains("AllTime", StringComparison.OrdinalIgnoreCase)) return "All time";
        if (name.Contains("Classic", StringComparison.OrdinalIgnoreCase)) return "Classic";
        if (name.Contains("Extreme", StringComparison.OrdinalIgnoreCase)) return "Extreme";
        return null;
    }

    private string? ReadLeaderboardVirtualMove(List<string> rows, bool allowUiMove)
    {
        if (rows.Count == 0)
            return null;

        int direction = allowUiMove ? ReadLeaderboardMoveDirection() : 0;
        long now = Environment.TickCount64;
        bool pageDown = Keyboard.current?.pageDownKey.wasPressedThisFrame == true;
        bool pageUp = Keyboard.current?.pageUpKey.wasPressedThisFrame == true;
        if (pageDown || pageUp)
            direction = pageDown ? 1 : -1;
        else
        {
            if (now < _leaderboardSuppressUiMoveUntilAt)
                return null;
            if (direction == 0)
            {
                _leaderboardMoveDirection = 0;
                return null;
            }
            if (direction == _leaderboardMoveDirection && now < _nextLeaderboardMoveAt)
                return null;
        }

        _leaderboardMoveDirection = direction;
        _nextLeaderboardMoveAt = now + 350;
        _leaderboardRowIndex = Math.Clamp(_leaderboardRowIndex + direction, 0, rows.Count - 1);
        string speech = $"{rows[_leaderboardRowIndex]}.";
        WriteStatus($"Leaderboard row {_leaderboardRowIndex + 1} of {rows.Count}: {speech}");
        return speech;
    }

    private static int ReadLeaderboardMoveDirection()
    {
        EventSystem? eventSystem = EventSystem.current;
        InputSystemUIInputModule? module = eventSystem?.GetComponent<InputSystemUIInputModule>();
        InputAction? move = module?.move?.action;
        if (move == null || !move.enabled)
            return 0;
        Vector2 vector = move.ReadValue<Vector2>();
        if (Math.Abs(vector.y) < 0.5f || Math.Abs(vector.y) < Math.Abs(vector.x))
            return 0;
        return vector.y > 0 ? -1 : 1;
    }

    private void ResetLeaderboardsFocus()
    {
        _leaderboardPanelId = 0;
        _leaderboardContext = null;
        _leaderboardRowsSignature = null;
        _leaderboardCachedRows.Clear();
        _nextLeaderboardRowsPollAt = 0;
        _leaderboardRowIndex = 0;
        _leaderboardSelectedId = 0;
        _leaderboardRowsDueAt = 0;
        _leaderboardRowsSpoken = false;
        _leaderboardEmptySpoken = false;
        _leaderboardWasLoading = false;
        _leaderboardMoveDirection = 0;
        _nextLeaderboardMoveAt = 0;
        _leaderboardSuppressUiMoveUntilAt = 0;
        _leaderboardPartyName = null;
        _leaderboardPartyState = null;
        _leaderboardContinueVisible = false;
        _leaderboardOuterVisibleAt = 0;
    }
}
