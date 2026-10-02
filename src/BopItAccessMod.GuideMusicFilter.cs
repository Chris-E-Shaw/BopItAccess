using Il2Cpp;
using Il2CppFMOD;
using Il2CppFMOD.Studio;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private bool _guideMusicFilterRequested;
    private bool _guideMusicFilterApplied;
    private EventInstance _guideFilteredMusicInstance;
    private PARAMETER_ID _guideMusicFilterParameter;
    private float _guideMusicFilterPreviousValue;
    private long _nextGuideMusicFilterAttemptAt;
    private long _nextGuideMusicFilterErrorAt;

    // The game's MusicManager already has a Filter parameter for its menu
    // music. The song-selection transition uses this FMOD parameter, so the
    // guide borrows it and restores the prior value when the reader closes.
    private void SetGuideMusicFilter(bool enabled)
    {
        _guideMusicFilterRequested = enabled;
        if (enabled)
        {
            _nextGuideMusicFilterAttemptAt = 0;
            UpdateGuideMusicFilter();
            return;
        }

        if (!_guideMusicFilterApplied)
            return;
        try
        {
            if (_guideFilteredMusicInstance.isValid())
            {
                RESULT result = _guideFilteredMusicInstance.setParameterByID(
                    _guideMusicFilterParameter, _guideMusicFilterPreviousValue,
                    false);
                WriteStatus("Guide music filter restored: " + result + ".");
            }
        }
        catch (Exception ex)
        {
            WriteStatus("Guide music filter restoration failed: " + ex.Message);
        }
        finally
        {
            _guideMusicFilterApplied = false;
        }
    }

    private void UpdateGuideMusicFilter()
    {
        if (!_guideMusicFilterRequested || _guideMusicFilterApplied ||
            Environment.TickCount64 < _nextGuideMusicFilterAttemptAt)
            return;
        _nextGuideMusicFilterAttemptAt = Environment.TickCount64 + 1000;

        try
        {
            MusicManager? music = UnityEngine.Object.FindFirstObjectByType<App>()?
                .MusicManager;
            if (music == null)
                return;
            EventInstance instance = music.menuMusicInstance;
            if (!instance.isValid())
                return;

            PARAMETER_ID parameter = music.filterParamID;
            RESULT read = instance.getParameterByID(parameter,
                out float previous);
            if (read != RESULT.OK)
            {
                LogGuideMusicFilterError("Guide music filter parameter unavailable: " +
                    read + ".");
                return;
            }

            // FMOD's Filter parameter is 0..1 in the installed game bank;
            // the filtered song-selection state is the upper endpoint.
            RESULT set = instance.setParameterByID(parameter, 1f, false);
            if (set != RESULT.OK)
            {
                LogGuideMusicFilterError("Guide music filter could not be applied: " +
                    set + ".");
                return;
            }
            _guideFilteredMusicInstance = instance;
            _guideMusicFilterParameter = parameter;
            _guideMusicFilterPreviousValue = previous;
            _guideMusicFilterApplied = true;
            WriteStatus("Applied guide music filter; previous value " +
                previous + ".");
        }
        catch (Exception ex)
        {
            LogGuideMusicFilterError("Guide music filter unavailable: " +
                ex.Message);
        }
    }

    private void LogGuideMusicFilterError(string message)
    {
        long now = Environment.TickCount64;
        if (now < _nextGuideMusicFilterErrorAt)
            return;
        _nextGuideMusicFilterErrorAt = now + 10000;
        WriteStatus(message);
    }
}
