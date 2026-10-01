using Il2Cpp;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private readonly Dictionary<string, string?> _nativeBindingGuardSnapshot = new();
    private int _nativeBindingGuardManagerId;
    private long _nextNativeBindingGuardErrorAt;

    // Interactive rebinds apply their candidate override before reporting
    // completion. Ignore only that exact binding, then inspect the active
    // native asset and every accessibility action before saving the change.
    private bool IsBindingAssignedElsewhere(InputRebindingManager manager,
        InputAction targetAction, int targetBindingIndex, string? candidatePath,
        out string owner)
    {
        owner = string.Empty;
        if (string.IsNullOrWhiteSpace(candidatePath))
            return false;

        InputActionAsset? native = manager.playerInput?.actions ?? manager.inputActions;
        if (native != null)
        {
            foreach (InputActionMap map in native.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    if (ActionHasConflictingBinding(action, targetAction,
                            targetBindingIndex, candidatePath))
                    {
                        owner = SpokenBindingOwner(action.name);
                        return true;
                    }
                }
            }
        }

        InputAction[] accessibilityActions =
        {
            EnsureDescriptionAction(), EnsureScoreAction(),
            EnsureToggleSpeechAction(), EnsureSpeakHintsAction(),
            EnsureChangeSpeechOutputAction()
        };
        foreach (InputAction action in accessibilityActions)
        {
            if (!ActionHasConflictingBinding(action, targetAction,
                    targetBindingIndex, candidatePath))
                continue;
            owner = SpokenBindingOwner(action.name);
            return true;
        }

        return false;
    }

    private static bool ActionHasConflictingBinding(InputAction action,
        InputAction targetAction, int targetBindingIndex, string candidatePath)
    {
        bool sameAction = action.id == targetAction.id;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (sameAction && i == targetBindingIndex)
                continue;

            InputBinding binding = action.bindings[i];
            if (binding.isComposite || BindingIsModifierChordPart(action, i))
                continue;
            // Reserve defaults as well as active overrides. Reset to Default
            // must not recreate a duplicate after a different action moved.
            if (DescriptionPathsMatch(binding.effectivePath, candidatePath) ||
                DescriptionPathsMatch(binding.path, candidatePath))
                return true;
        }
        return false;
    }

    private static bool BindingIsModifierChordPart(InputAction action, int index)
    {
        if (!action.bindings[index].isPartOfComposite)
            return false;
        int root = index - 1;
        while (root >= 0 && action.bindings[root].isPartOfComposite)
            root--;
        if (root < 0 || !action.bindings[root].isComposite)
            return false;
        // A plain key does not trigger the native Debug Menu or Invincible
        // action, both of which require an additional modifier button.
        return action.bindings[root].path?.IndexOf("Modifier",
            StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string SpokenBindingOwner(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "another control";
        var words = new System.Text.StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            char letter = name[i];
            if (i > 0 && char.IsUpper(letter) && char.IsLower(name[i - 1]))
                words.Append(' ');
            words.Append(letter);
        }
        return words.ToString();
    }

    // Native ControlRow rebinding does not know about mod-owned actions. Keep
    // an idle baseline so a native rebind that duplicates one can be undone
    // after the game's own completion callback, without patching that fragile
    // callback or interfering with its ordinary controls.
    private void UpdateNativeBindingConflictGuard()
    {
        try
        {
            UpdateNativeBindingConflictGuardCore();
        }
        catch (Exception ex)
        {
            if (Environment.TickCount64 >= _nextNativeBindingGuardErrorAt)
            {
                WriteStatus("Native binding conflict guard failed: " + ex);
                _nextNativeBindingGuardErrorAt = Environment.TickCount64 + 5000;
            }
        }
    }

    private void UpdateNativeBindingConflictGuardCore()
    {
        InputRebindingManager? manager = _controlsRebindingManager;
        Panel? panel = _mainMenu?.controlsPanel;
        if (manager == null || panel == null || !panel.IsVisible ||
            !panel.gameObject.activeInHierarchy || manager.inputActions == null)
        {
            _nativeBindingGuardSnapshot.Clear();
            _nativeBindingGuardManagerId = 0;
            return;
        }

        int managerId = manager.GetInstanceID();
        if (managerId != _nativeBindingGuardManagerId)
        {
            _nativeBindingGuardSnapshot.Clear();
            _nativeBindingGuardManagerId = managerId;
        }

        InputActionAsset asset = manager.inputActions;
        if (_nativeBindingGuardSnapshot.Count == 0)
        {
            CaptureNativeBindingSnapshot(asset);
            return;
        }

        if (manager.IsRebinding || _leaderboardRebindOperation != null ||
            _resetGyroRebindOperation != null)
            return;

        var changed = new List<(InputAction Action, int Index, string? Previous)>();
        foreach (InputActionMap map in asset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                for (int index = 0; index < action.bindings.Count; index++)
                {
                    string key = NativeBindingKey(action, index);
                    string? current = action.bindings[index].overridePath;
                    if (!_nativeBindingGuardSnapshot.TryGetValue(key, out string? previous))
                    {
                        _nativeBindingGuardSnapshot[key] = current;
                        continue;
                    }
                    if (!string.Equals(previous, current, StringComparison.Ordinal))
                        changed.Add((action, index, previous));
                }
            }
        }

        if (changed.Count == 0)
            return;

        string? conflictOwner = null;
        foreach (var item in changed)
        {
            InputBinding binding = item.Action.bindings[item.Index];
            if (binding.isComposite ||
                BindingIsModifierChordPart(item.Action, item.Index))
                continue;
            if (IsModBindingAssigned(binding.effectivePath, out string owner))
            {
                conflictOwner = owner;
                break;
            }
        }

        if (conflictOwner == null)
        {
            foreach (var item in changed)
                _nativeBindingGuardSnapshot[NativeBindingKey(item.Action, item.Index)] =
                    item.Action.bindings[item.Index].overridePath;
            return;
        }

        foreach (var item in changed)
        {
            RestoreNativeBindingOverride(item.Action, item.Index, item.Previous);
            InputAction? runtime = manager.playerInput?.actions?
                .FindActionMap(item.Action.actionMap.name, false)?
                .FindAction(item.Action.name, false);
            if (runtime != null && item.Index < runtime.bindings.Count)
                RestoreNativeBindingOverride(runtime, item.Index, item.Previous);
        }
        manager.SaveBindings();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        WriteStatus("Rejected native binding duplicate of " + conflictOwner + ".");
        QueueSpeech("That input is already assigned to " + conflictOwner +
            ". Binding unchanged");
    }

    private void CaptureNativeBindingSnapshot(InputActionAsset asset)
    {
        foreach (InputActionMap map in asset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                for (int index = 0; index < action.bindings.Count; index++)
                    _nativeBindingGuardSnapshot[NativeBindingKey(action, index)] =
                        action.bindings[index].overridePath;
            }
        }
    }

    private static string NativeBindingKey(InputAction action, int index) =>
        action.id.ToString() + ":" + index;

    private static void RestoreNativeBindingOverride(InputAction action, int index,
        string? path)
    {
        if (path == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index, path);
    }

    private bool IsModBindingAssigned(string? candidatePath, out string owner)
    {
        owner = string.Empty;
        if (string.IsNullOrWhiteSpace(candidatePath))
            return false;
        InputAction[] modActions =
        {
            EnsureDescriptionAction(), EnsureScoreAction(),
            EnsureToggleSpeechAction(), EnsureSpeakHintsAction(),
            EnsureChangeSpeechOutputAction()
        };
        foreach (InputAction action in modActions)
        {
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (!DescriptionPathsMatch(binding.effectivePath, candidatePath) &&
                    !DescriptionPathsMatch(binding.path, candidatePath))
                    continue;
                owner = SpokenBindingOwner(action.name);
                return true;
            }
        }
        return false;
    }
}
