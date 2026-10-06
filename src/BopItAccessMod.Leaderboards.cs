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
    private string _leaderboardCachedRowsSignature = string.Empty;
    private long _nextLeaderboardRowsPollAt;
    private int _leaderboardRowIndex;
    private int _leaderboardSelectedId;
    private long _leaderboardRowsDueAt;
    private bool _leaderboardRowsSpoken;
    private bool _leaderboardEmptySpoken;
    private int _leaderboardMoveDirection;
    private long _nextLeaderboardMoveAt;
    private long _leaderboardSuppressUiMoveUntilAt;
    private string? _leaderboardPartyName;
    private PartyLeaderboardState? _leaderboardPartyState;
    private bool _leaderboardContinueVisible;
    private long _leaderboardOuterVisibleAt;
    private string? _leaderboardTrack;
    private string? _leaderboardDevice;
    private string? _leaderboardGroup;
    private string? _leaderboardDate;

    // CombinedLeaderboardPanel serves both the main menu and the Solo result.
    // PartyLeaderboardPanel is the separate result screen with name selection.
    // Return true during loading too, so the underlying menu cannot speak.
    private bool ReadLeaderboardsFocus()
    {
        long now = Environment.TickCount64;
        if (_leaderboardMainMenu == null)
            _leaderboardMainMenu = null;
        if (_leaderboardGameUi == null)
            _leaderboardGameUi = null;
        if (now >= _nextLeaderboardSearchAt &&
            (_leaderboardMainMenu == null || _leaderboardGameUi == null))
        {
            _nextLeaderboardSearchAt = now + 500;
            if (_leaderboardMainMenu == null)
                _leaderboardMainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_leaderboardGameUi == null)
                _leaderboardGameUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
        }

        // The game keeps its result leaderboard wrappers alive while the song
        // selection screen is showing. Their Panel flags alone are not proof
        // that a leaderboard is the current screen.
        GameUIManager? gameUi = _leaderboardGameUi;
        if (gameUi != null && gameUi.gameManager != null &&
            gameUi.gameManager.GameState == GameState.GameOver &&
            !IsLeaderboardPanelVisible(gameUi.startScreen))
        {
            PartyLeaderboardPanel? party = gameUi.partyLeaderboardPanel;
            if (IsLeaderboardPanelVisible(party) &&
                IsLeaderboardPanelVisible(party?.leaderboardWrapper))
                return ReadPartyLeaderboard(party!);

            CombinedLeaderboardPanel? gameCombined = gameUi.combinedLeaderboardPanel;
            if (IsLeaderboardPanelVisible(gameCombined) &&
                IsLeaderboardPanelVisible(gameCombined?.leaderboardWrapper))
                return ReadCombinedLeaderboard(gameCombined!);
        }

        MainMenuUIManager? mainMenu = _leaderboardMainMenu;
        Panel? mainOuter = mainMenu != null ? mainMenu.leaderboardPanel : null;
        if (IsLeaderboardPanelVisible(mainOuter))
        {
            // Do not call GetComponentInChildren on a panel from a destroyed
            // main-menu scene. It can throw an IL2CPP NullReferenceException.
            CombinedLeaderboardPanel? mainCombined =
                mainOuter!.GetComponentInChildren<CombinedLeaderboardPanel>(true);
            if (mainCombined != null &&
                (mainCombined.leaderboardWrapper == null ||
                 IsLeaderboardPanelVisible(mainCombined.leaderboardWrapper)))
                return ReadCombinedLeaderboard(mainCombined);

            if (_leaderboardOuterVisibleAt == 0)
                _leaderboardOuterVisibleAt = now;
            if (now - _leaderboardOuterVisibleAt >= 500 && _leaderboardPanelId == 0)
            {
                _leaderboardPanelId = mainOuter!.GetInstanceID();
                QueueSpeech(L("Leaderboards loading."));
            }
            return true;
        }

        ResetLeaderboardsFocus();
        return false;
    }

    private static bool IsLeaderboardPanelVisible(Panel? panel) =>
        panel != null && panel.gameObject.activeInHierarchy &&
        (panel.IsVisible || panel.IsTransitioningShow) && !panel.IsTransitioningHide;

    private bool ReadCombinedLeaderboard(CombinedLeaderboardPanel panel)
    {
        int panelId = panel.GetInstanceID();
        if (panelId != _leaderboardPanelId)
        {
            ResetLeaderboardsFocus();
            _leaderboardPanelId = panelId;
        }

        long now = Environment.TickCount64;
        string? track = ReadLeaderboardTrack(panel.TitleImage, panel.LeaderboardManager, panel.app);
        string? device = ReadLeaderboardDevice(panel.deviceContainer, panel.LeaderboardManager);
        string group = ReadLeaderboardGroup(panel);
        string date = ReadLeaderboardDate(panel);
        string context = ReadCombinedLeaderboardContext(panel, track, device, group, date);
        (int focusedId, string? focusedText, int focusedIndex, int focusedCount) =
            ReadLeaderboardFocusedItem(panel.transform);
        bool focusChanged = focusedId != _leaderboardSelectedId;
        if (now >= _nextLeaderboardRowsPollAt || _leaderboardContext == null)
        {
            _leaderboardCachedRows = ReadCombinedLeaderboardRows(panel);
            _leaderboardCachedRowsSignature = string.Join("\n", _leaderboardCachedRows);
            _nextLeaderboardRowsPollAt = now + 100;
        }
        List<string> rows = _leaderboardCachedRows;
        string rowSignature = _leaderboardCachedRowsSignature;
        bool loading = IsLeaderboardLoading(panel.loader);
        bool contextChanged = !string.Equals(context, _leaderboardContext, StringComparison.Ordinal);
        bool rowsChanged = !string.Equals(rowSignature, _leaderboardRowsSignature, StringComparison.Ordinal);
        string? immediateSpeech = null;
        bool immediateIsFocus = false;
        if (contextChanged)
        {
            bool opening = _leaderboardContext == null;
            string? changedValue = null;
            string? changedValueType = null;
            int changedIndex = -1;
            int changedCount = 0;
            if (!string.Equals(track, _leaderboardTrack, StringComparison.Ordinal))
            {
                changedValue = track;
                changedValueType = null;
            }
            if (!string.Equals(device, _leaderboardDevice, StringComparison.Ordinal))
            {
                changedValue = device;
                changedValueType = null;
            }
            if (!string.Equals(group, _leaderboardGroup, StringComparison.Ordinal))
            {
                changedValue = group;
                changedValueType = "tab";
                GroupFilterTab? selectedGroup = group switch
                {
                    "Local" => panel.local,
                    "Friends" => panel.friends,
                    "Global" => panel.global,
                    _ => null
                };
                if (selectedGroup != null)
                    (changedIndex, changedCount) =
                        GetLeaderboardComponentIndex(panel.transform, selectedGroup);
            }
            if (!string.Equals(date, _leaderboardDate, StringComparison.Ordinal))
            {
                changedValue = date;
                changedValueType = "tab";
                DateFilterTab? selectedDate = date switch
                {
                    "Today" => panel.today,
                    "This month" => panel.month,
                    "All time" => panel.allTime,
                    _ => null
                };
                if (selectedDate != null)
                    (changedIndex, changedCount) =
                        GetLeaderboardComponentIndex(panel.transform, selectedDate);
            }
            _leaderboardTrack = track;
            _leaderboardDevice = device;
            _leaderboardGroup = group;
            _leaderboardDate = date;
            _leaderboardContext = context;
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 800;
            _leaderboardMoveDirection = 0;
            _leaderboardSuppressUiMoveUntilAt = now + 500;
            immediateSpeech = opening
                ? context + L(".") +
                    (focusedText == null ? string.Empty : " " + WithMenuIndex(
                        WithLeaderboardFocusedType(focusedText, panel.transform),
                        focusedIndex, focusedCount) + L("."))
                : focusChanged && IsLeaderboardFilterLabel(focusedText)
                    ? WithMenuIndex(WithLeaderboardFocusedType(focusedText!, panel.transform),
                        focusedIndex, focusedCount)
                    : (changedValue != null
                        ? WithMenuIndex(changedValueType == null ? L(changedValue) :
                            WithControlType(L(changedValue), changedValueType),
                            changedIndex, changedCount) : null)
                        ?? (focusChanged && focusedText != null
                        ? WithMenuIndex(WithLeaderboardFocusedType(focusedText, panel.transform),
                            focusedIndex, focusedCount) : null) ?? context;
            immediateIsFocus = opening ||
                (focusChanged && focusedText != null &&
                 (IsLeaderboardFilterLabel(focusedText) || changedValue == null));
            WriteStatus($"Leaderboard context: {context}.");
        }
        else if (rowsChanged)
        {
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 500;
            _leaderboardSuppressUiMoveUntilAt = now + 200;
        }

        string? rowAnnouncement = ReadLeaderboardRowsAfterLoad(rows,
            loading, panel.leaderboardEntryContainerPanel, now);
        if (focusChanged)
        {
            _leaderboardSelectedId = focusedId;
            if (focusedText != null)
            {
                if (!contextChanged)
                {
                    immediateSpeech = WithMenuIndex(
                        WithLeaderboardFocusedType(focusedText, panel.transform),
                        focusedIndex, focusedCount);
                    immediateIsFocus = true;
                }
                WriteStatus($"Leaderboard focus: {focusedText}.");
            }
        }

        if (!contextChanged && immediateSpeech == null && rowAnnouncement == null)
        {
            string? movedRow = ReadLeaderboardVirtualMove(rows, focusedText == null);
            if (movedRow != null)
                immediateSpeech = movedRow;
        }
        if (immediateSpeech != null)
        {
            if (immediateIsFocus)
                QueueFocusSpeech(immediateSpeech);
            else
                QueueSpeech(immediateSpeech);
        }
        if (rowAnnouncement != null)
            QueueSequentialSpeech(rowAnnouncement);
        return true;
    }

    private static bool IsLeaderboardFilterLabel(string? label)
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected?.GetComponentInParent<GroupFilterTab>() != null ||
            selected?.GetComponentInParent<DateFilterTab>() != null)
            return true;
        return label != null && new[] { "Local", "Friends", "Global",
            "Today", "This month", "All time" }
            .Any(filter => label == filter || label == L(filter));
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
            _leaderboardCachedRowsSignature = string.Join("\n", _leaderboardCachedRows);
            _nextLeaderboardRowsPollAt = now + 100;
        }
        List<string> rows = _leaderboardCachedRows;
        string rowSignature = _leaderboardCachedRowsSignature;
        bool loading = IsLeaderboardLoading(panel.loader);
        bool contextChanged = !string.Equals(context, _leaderboardContext, StringComparison.Ordinal);
        bool rowsChanged = !string.Equals(rowSignature, _leaderboardRowsSignature, StringComparison.Ordinal);
        var announcements = new List<string>(5);
        bool focusAnnouncement = false;
        (int focusedId, string? focusedText, int focusedIndex, int focusedCount) =
            ReadLeaderboardFocusedItem(panel.transform);
        if (contextChanged)
        {
            _leaderboardContext = context;
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 800;
            _leaderboardSelectedId = focusedId;
            _leaderboardMoveDirection = 0;
            _leaderboardSuppressUiMoveUntilAt = now + 500;
            _leaderboardPartyState = panel.State;
            _leaderboardPartyName = ReadPartySelectedName(panel);
            _leaderboardContinueVisible = IsLeaderboardPanelVisible(panel.ContinuePrompt);
            string initialName = _leaderboardPartyName == null ? string.Empty :
                " " + LF("Selected name: {0}.", _leaderboardPartyName);
            announcements.Add(context + L(".") + initialName +
                (focusedText == null ? string.Empty : " " + WithMenuIndex(
                    WithLeaderboardFocusedType(focusedText, panel.transform),
                    focusedIndex, focusedCount) + L(".")));
            focusAnnouncement = true;
            WriteStatus($"Party leaderboard context: {context}.");
        }
        else if (rowsChanged)
        {
            _leaderboardRowsSignature = rowSignature;
            _leaderboardRowIndex = 0;
            _leaderboardRowsSpoken = false;
            _leaderboardEmptySpoken = false;
            _leaderboardRowsDueAt = now + 500;
            _leaderboardSuppressUiMoveUntilAt = now + 200;
        }

        string? rowAnnouncement = ReadLeaderboardRowsAfterLoad(rows,
            loading, panel.leaderboardEntryContainerPanel, now);

        string? name = ReadPartySelectedName(panel);
        PartyLeaderboardState state = panel.State;
        bool continueVisible = IsLeaderboardPanelVisible(panel.ContinuePrompt);
        if (state != _leaderboardPartyState)
        {
            _leaderboardPartyState = state;
            _leaderboardPartyName = name;
            string stateSpeech = state switch
            {
                PartyLeaderboardState.Input => name == null ? L("Enter a name.") :
                    LF("Enter a name. {0}.", name),
                PartyLeaderboardState.Complete => L("Name confirmed. Continue."),
                _ => name == null ? L("Choose a name.") : LF("Choose a name. {0}.", name)
            };
            if (!contextChanged)
                announcements.Add(stateSpeech);
            WriteStatus($"Party leaderboard state: {stateSpeech}");
        }
        else if (name != null && !string.Equals(name, _leaderboardPartyName, StringComparison.Ordinal))
        {
            _leaderboardPartyName = name;
            if (!contextChanged)
                announcements.Add(LF("Name: {0}.", name));
        }

        if (continueVisible && !_leaderboardContinueVisible && !contextChanged)
            announcements.Add(L("Continue."));
        _leaderboardContinueVisible = continueVisible;

        if (focusedId != _leaderboardSelectedId)
        {
            _leaderboardSelectedId = focusedId;
            if (focusedText != null && !contextChanged)
            {
                announcements.Add(WithMenuIndex(
                    WithLeaderboardFocusedType(focusedText, panel.transform),
                    focusedIndex, focusedCount));
                focusAnnouncement = true;
            }
        }

        if (!contextChanged && announcements.Count == 0 && rowAnnouncement == null &&
            state != PartyLeaderboardState.Input)
        {
            string? movedRow = ReadLeaderboardVirtualMove(rows, focusedText == null);
            if (movedRow != null)
                announcements.Add(movedRow);
        }
        if (announcements.Count > 0)
        {
            string announcement = string.Join(" ", announcements);
            if (focusAnnouncement)
                QueueFocusSpeech(announcement);
            else
                QueueSpeech(announcement);
        }
        if (rowAnnouncement != null)
            QueueSequentialSpeech(rowAnnouncement);
        return true;
    }

    private static string ReadCombinedLeaderboardContext(CombinedLeaderboardPanel panel,
        string? track, string? device, string group, string date)
    {
        string location = panel.Mode == CombinedLeaderboardMode.Game
            ? L("Solo leaderboard") : L("Leaderboards");
        string offline = panel.LeaderboardManager != null &&
            !panel.LeaderboardManager.isOnline ? L("offline") : string.Empty;
        return string.Join(L(", "), new[] { location, L(track ?? string.Empty),
            L(device ?? string.Empty), L(group), L(date), offline }
            .Where(part => !string.IsNullOrEmpty(part)));
    }

    private static string ReadPartyLeaderboardContext(PartyLeaderboardPanel panel)
    {
        LeaderboardManager? manager = panel.LeaderboardManager;
        string? track = ReadLeaderboardTrack(panel.TitleImage, manager, manager?.app);
        string? device = ReadLeaderboardDevice(manager?.DeviceContainer, manager);
        return string.Join(L(", "), new[] { L("Party leaderboard"),
            L(track ?? string.Empty), L(device ?? string.Empty) }
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
                CleanSpeechValue(score?.Identity?.Alias) ?? L("Unnamed player");
            string value = score == null
                ? CleanSpeechValue(entry.ScoreText?.text) ?? L("unknown")
                : score.Score.ToString();
            string owner = entry.ItemType switch
            {
                LeaderboardItemType.Me => L("your score, "),
                LeaderboardItemType.MostRecent => L("most recent score, "),
                _ => string.Empty
            };
            rows.Add(LF("Rank {0}, {1}{2}, score {3}", rank, owner, alias, value));
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
                CleanSpeechValue(score?.Identity?.Alias) ?? L("Unnamed player");
            string value = score == null
                ? CleanSpeechValue(entry.ScoreText?.text) ?? L("unknown")
                : score.Score.ToString();
            string current = entry.ItemType == PartyLeaderboardItemType.Current ?
                L("current score, ") : string.Empty;
            rows.Add(LF("Rank {0}, {1}{2}, score {3}", rank, current, alias, value));
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
            string announcement = LF("{0} scores.", rows.Count) + " " +
                WithMenuIndex(WithLeaderboardRowType(rows[0]), 0, rows.Count) + L(".");
            WriteStatus($"Leaderboard rows loaded: {rows.Count}; first row: {rows[0]}.");
            return announcement;
        }
        else if (!_leaderboardEmptySpoken && now >= _leaderboardRowsDueAt + 900)
        {
            _leaderboardEmptySpoken = true;
            WriteStatus("Leaderboard has no displayed scores.");
            return L("No scores available.");
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

    private static (int Id, string? Label, int Index, int Count)
        ReadLeaderboardFocusedItem(Transform panel)
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(panel))
            return (0, null, -1, 0);

        LeaderboardLineItem? regular = selected.GetComponentInParent<LeaderboardLineItem>();
        if (regular != null)
        {
            string label = LF("Rank {0}, {1}, score {2}", regular.Rank,
                CleanSpeechValue(regular.AliasText?.text) ?? L("Unnamed player"),
                regular.Score?.Score.ToString() ??
                CleanSpeechValue(regular.ScoreText?.text) ?? L("unknown"));
            var position = GetLeaderboardComponentIndex(panel, regular,
                item => item.Score != null);
            return (regular.GetInstanceID(), label, position.Index, position.Count);
        }

        PartyLeaderboardNameSelect? nameSelect =
            selected.GetComponentInParent<PartyLeaderboardNameSelect>();
        if (nameSelect != null)
        {
            string? name = CleanSpeechValue(nameSelect.NameInput?.text) ??
                CleanSpeechValue(nameSelect.playerAlias) ??
                CleanSpeechValue(nameSelect.Label?.text);
            Selectable? nameControl = selected.GetComponentInParent<Selectable>();
            var position = nameControl == null ? (-1, 0) :
                GetLeaderboardComponentIndex(panel, nameControl,
                    item => item.IsInteractable());
            return (nameSelect.GetInstanceID(), name == null ? L("Choose name") :
                LF("Name: {0}", name),
                position.Item1, position.Item2);
        }

        PartyLeaderboardLineItem? party = selected.GetComponentInParent<PartyLeaderboardLineItem>();
        if (party != null)
        {
            string label = LF("Rank {0}, {1}, score {2}", party.Rank,
                CleanSpeechValue(party.AliasText?.text) ?? L("Unnamed player"),
                party.Score?.Score.ToString() ??
                CleanSpeechValue(party.ScoreText?.text) ?? L("unknown"));
            var position = GetLeaderboardComponentIndex(panel, party,
                item => item.Score != null);
            return (party.GetInstanceID(), label, position.Index, position.Count);
        }

        GroupFilterTab? group = selected.GetComponentInParent<GroupFilterTab>();
        if (group != null)
        {
            string? label = DescribeLeaderboardControlName(group.name) ??
                NormalizeLeaderboardFilterLabel(CleanSpeechValue(group.Text?.text));
            var position = GetLeaderboardComponentIndex(panel, group);
            return (group.GetInstanceID(), label, position.Index, position.Count);
        }

        DateFilterTab? date = selected.GetComponentInParent<DateFilterTab>();
        if (date != null)
        {
            var position = GetLeaderboardComponentIndex(panel, date);
            return (date.GetInstanceID(),
                DescribeLeaderboardControlName(date.name) ??
                NormalizeLeaderboardFilterLabel(
                    CleanSpeechValue(date.GetComponentInChildren<TMP_Text>(true)?.text)),
                position.Index, position.Count);
        }

        Selectable? control = selected.GetComponentInParent<Selectable>();
        if (control == null || !control.transform.IsChildOf(panel))
            return (selected.GetInstanceID(),
                CleanSpeechValue(selected.GetComponentInChildren<TMP_Text>(true)?.text) ??
                DescribeLeaderboardControlName(selected.name), -1, 0);

        string? labelText = CleanSpeechValue(control.GetComponentInChildren<TMP_Text>(true)?.text);
        labelText ??= DescribeLeaderboardControlName(control.name);
        var controlPosition = GetLeaderboardComponentIndex(panel, control,
            item => item.IsInteractable());
        return (control.GetInstanceID(), labelText,
            controlPosition.Index, controlPosition.Count);
    }

    private string WithLeaderboardFocusedType(string label, Transform panel)
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(panel))
            return L(label);
        if (selected.GetComponentInParent<LeaderboardLineItem>() != null ||
            selected.GetComponentInParent<PartyLeaderboardLineItem>() != null)
            return WithLeaderboardRowType(label);
        PartyLeaderboardNameSelect? nameSelect =
            selected.GetComponentInParent<PartyLeaderboardNameSelect>();
        if (nameSelect != null)
        {
            string? name = CleanSpeechValue(nameSelect.NameInput?.text) ??
                CleanSpeechValue(nameSelect.playerAlias) ??
                CleanSpeechValue(nameSelect.Label?.text);
            return name == null ? WithControlType(L("Choose name"), "text field") :
                LF("{0}, {1}", WithControlType(L("Name"), "text field"), name);
        }
        if (selected.GetComponentInParent<GroupFilterTab>() != null ||
            selected.GetComponentInParent<DateFilterTab>() != null)
            return WithControlType(L(label), "tab");
        Slider? slider = selected.GetComponentInParent<Slider>();
        if (slider != null)
            return WithUnitySliderRange(WithControlType(L(label), "slider"), slider);
        Scrollbar? scrollbar = selected.GetComponentInParent<Scrollbar>();
        if (scrollbar != null)
            return WithUnitySliderRange(WithControlType(L(label), "slider"), scrollbar);
        if (selected.GetComponentInParent<Toggle>() != null)
            return WithControlType(L(label), "toggle");
        return WithControlType(L(label), "button");
    }

    private string WithLeaderboardRowType(string row)
    {
        string scoreSeparator = L(", score ");
        int scoreAt = row.LastIndexOf(scoreSeparator, StringComparison.OrdinalIgnoreCase);
        if (scoreAt < 0)
            scoreAt = row.LastIndexOf(", score ", StringComparison.OrdinalIgnoreCase);
        return scoreAt < 0 ? WithControlType(row, "list item") :
            WithControlType(row[..scoreAt], "list item") + row[scoreAt..];
    }

    private static (int Index, int Count) GetLeaderboardComponentIndex<T>(
        Transform panel, T focused, Func<T, bool>? include = null) where T : Component
    {
        int index = -1;
        int count = 0;
        foreach (T item in panel.GetComponentsInChildren<T>(true))
        {
            if (item == null || !item.gameObject.activeInHierarchy ||
                (include != null && !include(item)))
                continue;
            if (item.GetInstanceID() == focused.GetInstanceID())
                index = count;
            count++;
        }
        return (index, count);
    }

    private static string? DescribeLeaderboardControlName(string name)
    {
        string normalized = name.Replace(" ", string.Empty).Replace("_", string.Empty);
        if (normalized.Contains("Back", StringComparison.OrdinalIgnoreCase)) return "Back";
        if (normalized.Contains("Continue", StringComparison.OrdinalIgnoreCase)) return "Continue";
        if (normalized.Contains("Local", StringComparison.OrdinalIgnoreCase)) return "Local";
        if (normalized.Contains("Friend", StringComparison.OrdinalIgnoreCase)) return "Friends";
        if (normalized.Contains("Global", StringComparison.OrdinalIgnoreCase)) return "Global";
        if (normalized.Contains("Today", StringComparison.OrdinalIgnoreCase)) return "Today";
        if (normalized.Contains("Month", StringComparison.OrdinalIgnoreCase)) return "This month";
        if (normalized.Contains("AllTime", StringComparison.OrdinalIgnoreCase)) return "All time";
        if (normalized.Contains("Classic", StringComparison.OrdinalIgnoreCase)) return "Classic";
        if (normalized.Contains("Extreme", StringComparison.OrdinalIgnoreCase)) return "Extreme";
        return null;
    }

    private static string? NormalizeLeaderboardFilterLabel(string? text)
    {
        if (text == null) return null;
        return DescribeLeaderboardControlName(text) ?? text;
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
        string speech = WithMenuIndex(WithLeaderboardRowType(rows[_leaderboardRowIndex]),
            _leaderboardRowIndex, rows.Count) + L(".");
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
        _leaderboardCachedRowsSignature = string.Empty;
        _nextLeaderboardRowsPollAt = 0;
        _leaderboardRowIndex = 0;
        _leaderboardSelectedId = 0;
        _leaderboardRowsDueAt = 0;
        _leaderboardRowsSpoken = false;
        _leaderboardEmptySpoken = false;
        _leaderboardTrack = null;
        _leaderboardDevice = null;
        _leaderboardGroup = null;
        _leaderboardDate = null;
        _leaderboardMoveDirection = 0;
        _nextLeaderboardMoveAt = 0;
        _leaderboardSuppressUiMoveUntilAt = 0;
        _leaderboardPartyName = null;
        _leaderboardPartyState = null;
        _leaderboardContinueVisible = false;
        _leaderboardOuterVisibleAt = 0;
    }
}
