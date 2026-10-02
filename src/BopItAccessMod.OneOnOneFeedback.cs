using Il2Cpp;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string OneOnOneFeedbackPreferenceKey =
        "BopItAccess.OneOnOneFeedback";

    private enum OneOnOneColour { Yellow, Green }

    private bool _oneOnOneFeedbackEnabled = true;
    private GameManager? _oneOnOneFeedbackGame;
    private GameUIManager? _oneOnOneFeedbackUi;
    private int _oneOnOneFeedbackGameId;
    private bool _oneOnOneFeedbackWasPlaying;
    private int _oneOnOneLastYellowLives;
    private int _oneOnOneLastGreenLives;
    private OneOnOneColour? _oneOnOneLastColour;
    private long _oneOnOneLastLifeSpeechAt;
    private long _nextOneOnOneFeedbackSearchAt;
    private long _nextOneOnOneFeedbackErrorAt;

    private void InitializeOneOnOneFeedbackPreferenceOnMainThread()
    {
        try
        {
            _oneOnOneFeedbackEnabled =
                PlayerPrefs.GetInt(OneOnOneFeedbackPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            _oneOnOneFeedbackEnabled = true;
            WriteStatus("Could not read one-on-one feedback preference: " + ex.Message);
        }

        WriteStatus("One-on-one feedback: " +
            (_oneOnOneFeedbackEnabled ? "On" : "Off") + ".");
    }

    private void SetOneOnOneFeedbackFromMenu(bool enabled)
    {
        if (_oneOnOneFeedbackEnabled == enabled)
            return;

        _oneOnOneFeedbackEnabled = enabled;
        ResetOneOnOneFeedbackSession();
        try
        {
            PlayerPrefs.SetInt(OneOnOneFeedbackPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save one-on-one feedback preference: " + ex.Message);
        }

        WriteStatus("One-on-one feedback " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    // GameManager owns the actual life counters. The game's OneOnOneScorePanel
    // renders Score as yellow's lives and Player2Score as green's lives. Using
    // those counters avoids transient UI animation or stale toggle states.
    private void UpdateOneOnOneFeedback()
    {
        if (!_oneOnOneFeedbackEnabled)
            return;

        try
        {
            GameManager? game = _oneOnOneFeedbackGame;
            if (game == null)
            {
                ResetOneOnOneFeedbackSession();
                long now = Environment.TickCount64;
                if (now < _nextOneOnOneFeedbackSearchAt)
                    return;

                _nextOneOnOneFeedbackSearchAt = now + 500;
                game = UnityEngine.Object.FindFirstObjectByType<GameManager>();
                if (game == null)
                    return;
                _oneOnOneFeedbackGame = game;
            }

            int gameId = game.GetInstanceID();
            if (gameId != _oneOnOneFeedbackGameId)
            {
                ResetOneOnOneFeedbackSession();
                _oneOnOneFeedbackGameId = gameId;
            }

            if (game.GameMode != GameMode.OneOnOne)
            {
                ResetOneOnOneFeedbackSession();
                return;
            }

            GameState state = game.GameState;
            if (state == GameState.Paused)
                return;
            if (state != GameState.Playing)
            {
                ResetOneOnOneFeedbackSession();
                return;
            }

            // The song-selection reader clears its speech when it observes
            // gameplay in OnLateUpdate. Wait for that transition to finish so
            // it cannot erase the first colour announced here in OnUpdate.
            if (_trackSelectWasVisible || _trackSelectStartTransitionUntil != 0)
                return;

            // PlayingStart shows this panel for one-on-one mode. Its visible
            // state distinguishes real play from the short state transition
            // before the game has presented the first instruction.
            if (_oneOnOneFeedbackUi == null)
                _oneOnOneFeedbackUi =
                    UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
            Panel? scorePanel =
                _oneOnOneFeedbackUi?.oneOnOneScorePanel;
            if (scorePanel == null || !scorePanel.IsVisible ||
                !scorePanel.gameObject.activeInHierarchy)
                return;

            int yellowLives = game.Score;
            int greenLives = game.Player2Score;
            OneOnOneColour? colour = ReadOneOnOneInstructionColour(
                game.CurrentInstruction);

            if (!_oneOnOneFeedbackWasPlaying)
            {
                _oneOnOneFeedbackWasPlaying = true;
                _oneOnOneLastYellowLives = yellowLives;
                _oneOnOneLastGreenLives = greenLives;
                _oneOnOneLastColour = colour;
                if (colour.HasValue)
                {
                    QueueSpeech(L(colour.Value.ToString()));
                    WriteStatus("One-on-one starting colour: " + colour.Value + ".");
                }
                return;
            }

            var announcements = new List<string>(3);
            bool lifeChanged = false;
            // A score of zero ends the round. Leave the winner announcement
            // to the existing game-over reader rather than speaking over it.
            bool ending = yellowLives <= 0 || greenLives <= 0;
            if (!ending)
            {
                bool yellowLifeChanged = yellowLives != _oneOnOneLastYellowLives;
                bool greenLifeChanged = greenLives != _oneOnOneLastGreenLives;
                bool bothLivesChanged = yellowLifeChanged && greenLifeChanged;
                lifeChanged = yellowLifeChanged || greenLifeChanged;
                if (yellowLifeChanged)
                    announcements.Add((bothLivesChanged ? L("Yellow") + ", " : "") +
                        FormatOneOnOneLives(yellowLives));
                if (greenLifeChanged)
                    announcements.Add((bothLivesChanged ? L("Green") + ", " : "") +
                        FormatOneOnOneLives(greenLives));
            }

            _oneOnOneLastYellowLives = yellowLives;
            _oneOnOneLastGreenLives = greenLives;

            // Bop and Alt Bop are shared actions. Hold the previous definite
            // side through them; guessing a colour there would mislead play.
            if (colour.HasValue && colour != _oneOnOneLastColour)
            {
                announcements.Add(L(colour.Value.ToString()));
                _oneOnOneLastColour = colour;
            }

            if (!ending && announcements.Count != 0)
            {
                string speech = string.Join(". ", announcements);
                long now = Environment.TickCount64;
                // A colour switch can be published a frame after a life change.
                // Queue it after the short life count so both remain audible.
                if (!lifeChanged && _oneOnOneLastLifeSpeechAt != 0 &&
                    now - _oneOnOneLastLifeSpeechAt < 1200)
                    QueueSequentialSpeech(speech);
                else
                    QueueSpeech(speech);
                if (lifeChanged)
                    _oneOnOneLastLifeSpeechAt = now;
                WriteStatus("One-on-one feedback: " + speech + ".");
            }
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextOneOnOneFeedbackErrorAt)
            {
                _nextOneOnOneFeedbackErrorAt = now + 5000;
                WriteStatus("One-on-one feedback check failed: " + ex);
            }
            _oneOnOneFeedbackGame = null;
            _oneOnOneFeedbackUi = null;
            ResetOneOnOneFeedbackSession();
        }
    }

    private static OneOnOneColour? ReadOneOnOneInstructionColour(
        ActionType instruction) => instruction switch
    {
        ActionType.Twist or ActionType.Pull => OneOnOneColour.Yellow,
        ActionType.Spin or ActionType.Flick => OneOnOneColour.Green,
        _ => null
    };

    private static string FormatOneOnOneLives(int count) =>
        count == 1 ? L("1 life") : LF("{0} lives", count);

    private void ResetOneOnOneFeedbackSession()
    {
        _oneOnOneFeedbackWasPlaying = false;
        _oneOnOneFeedbackGameId = 0;
        _oneOnOneLastYellowLives = 0;
        _oneOnOneLastGreenLives = 0;
        _oneOnOneLastColour = null;
        _oneOnOneLastLifeSpeechAt = 0;
    }
}
