using System.Reflection;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppFMODUnity;
using Il2CppInterop.Runtime;
using Il2CppSweetLibs.UI;
using UnityEngine;
using UnityEngine.Events;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private static BopItAccessMod? _activeMenuSelectionAudioMod;
    private readonly Dictionary<IntPtr, MenuSelectionAudioEntry> _menuSelectionAudio = new();
    private long _nextMenuSelectionAudioErrorAt;

    // A visual-state callback can invoke the same serialized sound listener
    // without a new focus/hover entry. Scope just that listener, keeping the
    // game's visual effects and distinct confirmation sounds intact.
    [ThreadStatic]
    private static MenuSelectionAudioScope? _menuSelectionAudioScope;

    internal sealed class MenuSelectionAudioEntry
    {
        internal readonly SelectableEvents Events;
        internal readonly StudioEventEmitter Emitter;
        internal readonly string Name;
        internal UnityEvent? RepairedEvent;
        internal UnityAction? RepairListener;
        internal int LastSoundFrame = -1;
        internal string? LastSoundReason;

        internal MenuSelectionAudioEntry(SelectableEvents events,
            StudioEventEmitter emitter) =>
            (Events, Emitter, Name) = (events, emitter, events.gameObject.name);
    }

    internal struct MenuSelectionAudioScope
    {
        internal MenuSelectionAudioEntry Entry;
        internal string Reason;
        internal bool AllowSound;
        internal bool Played;
    }

    private void RegisterMenuSelectionAudio(SelectableEvents events)
    {
        if (_modStopping || _menuSelectionAudio.ContainsKey(events.Pointer) ||
            events.GetComponent<MainMenuButtonFX>() == null)
            return;

        // Discover the game's own selection emitter from the hover callback;
        // no event GUID, audio file, volume or FMOD instance is substituted.
        StudioEventEmitter? emitter = FindMenuSelectionEmitter(events.Highlighted,
            events.gameObject);
        if (emitter == null || events.Selected == null)
            return;

        foreach (IntPtr key in _menuSelectionAudio
                     .Where(item => item.Value.Events == null)
                     .Select(item => item.Key).ToArray())
            _menuSelectionAudio.Remove(key);

        var entry = new MenuSelectionAudioEntry(events, emitter);
        if (!HasMenuSelectionListener(events.Selected, emitter))
        {
            // The shipped Play button has the visual Selected callback but
            // lacks this Play listener. Add only the missing runtime listener.
            entry.RepairedEvent = events.Selected;
            entry.RepairListener = DelegateSupport.ConvertDelegate<UnityAction>(
                (Action)(() => PlayRepairedMenuSelectionSound(entry)));
            entry.RepairedEvent.AddListener(entry.RepairListener);
            WriteStatus("Restored missing menu selection sound listener: " + entry.Name + ".");
        }
        _menuSelectionAudio.Add(events.Pointer, entry);
    }

    private void PlayRepairedMenuSelectionSound(MenuSelectionAudioEntry entry)
    {
        if (_modStopping || entry.Events == null || entry.Emitter == null)
            return;
        try { entry.Emitter.Play(); }
        catch (Exception ex) { LogMenuSelectionAudioError(ex); }
    }

    private static StudioEventEmitter? FindMenuSelectionEmitter(UnityEvent? callbacks,
        GameObject owner)
    {
        if (callbacks == null)
            return null;
        for (int index = 0; index < callbacks.GetPersistentEventCount(); index++)
        {
            if (callbacks.GetPersistentMethodName(index) != "Play" ||
                callbacks.GetPersistentListenerState(index) == UnityEventCallState.Off)
                continue;
            StudioEventEmitter? emitter = callbacks.GetPersistentTarget(index)?
                .TryCast<StudioEventEmitter>();
            if (emitter != null && emitter.gameObject == owner)
                return emitter;
        }
        return null;
    }

    private static bool HasMenuSelectionListener(UnityEvent callbacks,
        StudioEventEmitter emitter)
    {
        for (int index = 0; index < callbacks.GetPersistentEventCount(); index++)
            if (callbacks.GetPersistentMethodName(index) == "Play" &&
                callbacks.GetPersistentListenerState(index) != UnityEventCallState.Off &&
                callbacks.GetPersistentTarget(index)?.Pointer == emitter.Pointer)
                return true;
        return false;
    }

    internal static void NoteMenuSelectionAudioAwake(SelectableEvents events)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeMenuSelectionAudioMod);
        if (mod == null || mod._modStopping)
            return;
        try { mod.RegisterMenuSelectionAudio(events); }
        catch (Exception ex) { mod.LogMenuSelectionAudioError(ex); }
    }

    internal static MenuSelectionAudioScope? BeginMenuSelectionAudioCallback(
        SelectableEvents events, string reason)
    {
        MenuSelectionAudioScope? previous = _menuSelectionAudioScope;
        _menuSelectionAudioScope = null;
        BopItAccessMod? mod = Volatile.Read(ref _activeMenuSelectionAudioMod);
        if (mod == null || mod._modStopping)
            return previous;
        try
        {
            // Awake normally registers these ten buttons. Also cover a late
            // attachment on an actual entry without scanning every Update.
            if (reason is "OnSelect" or "OnPointerEnter")
                mod.RegisterMenuSelectionAudio(events);
            if (!mod._menuSelectionAudio.TryGetValue(events.Pointer, out var entry))
                return previous;

            if (reason == "OnDeselect")
            {
                // A genuine away/back transition may happen in one frame.
                entry.LastSoundFrame = -1;
                entry.LastSoundReason = null;
            }
            bool allow = events.selectableComponent?.interactable == true &&
                (reason == "OnSelect" && !events.hasSelection ||
                 reason == "OnPointerEnter" && !events.isPointerInside && !events.hasSelection);
            // Hover and selection can enter together. This only coalesces
            // that pair, never separate rapid navigation presses. Selecting
            // an item with a stationary pointer remains audible on re-entry.
            if (reason == "OnSelect" && events.isPointerInside &&
                entry.LastSoundFrame == Time.frameCount &&
                entry.LastSoundReason == "OnPointerEnter")
                allow = false;
            _menuSelectionAudioScope = new MenuSelectionAudioScope
            {
                Entry = entry, Reason = reason, AllowSound = allow
            };
        }
        catch (Exception ex) { mod.LogMenuSelectionAudioError(ex); }
        return previous;
    }

    internal static void EndMenuSelectionAudioCallback(MenuSelectionAudioScope? previous) =>
        _menuSelectionAudioScope = previous;

    internal static bool AllowMenuSelectionSound(StudioEventEmitter emitter)
    {
        MenuSelectionAudioScope? current = _menuSelectionAudioScope;
        if (!current.HasValue || current.Value.Entry.Emitter.Pointer != emitter.Pointer)
            return true;
        BopItAccessMod? mod = Volatile.Read(ref _activeMenuSelectionAudioMod);
        if (mod == null || mod._modStopping)
            return true;
        try
        {
            MenuSelectionAudioScope scope = current.Value;
            if (!scope.AllowSound || scope.Played)
            {
                WriteStatus("Suppressed repeated menu selection sound: " +
                    scope.Entry.Name + ", " + scope.Reason + ".");
                return false;
            }
            scope.Played = true;
            _menuSelectionAudioScope = scope;
            scope.Entry.LastSoundFrame = Time.frameCount;
            scope.Entry.LastSoundReason = scope.Reason;
            WriteStatus("Menu selection sound requested: " + scope.Entry.Name +
                ", " + scope.Reason + ".");
            return true;
        }
        catch (Exception ex)
        {
            mod.LogMenuSelectionAudioError(ex);
            return true; // Optional sound repair must fail open to native audio.
        }
    }

    private void LogMenuSelectionAudioError(Exception error)
    {
        long now = Environment.TickCount64;
        if (now < _nextMenuSelectionAudioErrorAt)
            return;
        _nextMenuSelectionAudioErrorAt = now + 5000;
        WriteStatus("Menu selection audio repair failed; retaining native audio: " + error.Message);
    }

    private void StopMenuSelectionAudio()
    {
        Interlocked.CompareExchange(ref _activeMenuSelectionAudioMod, null, this);
        _menuSelectionAudioScope = null;
        foreach (MenuSelectionAudioEntry entry in _menuSelectionAudio.Values)
        {
            try
            {
                if (entry.Events != null && entry.RepairedEvent != null &&
                    entry.RepairListener != null)
                    entry.RepairedEvent.RemoveListener(entry.RepairListener);
            }
            catch (Exception ex) { LogMenuSelectionAudioError(ex); }
        }
        _menuSelectionAudio.Clear();
    }
}

[HarmonyPatch(typeof(SelectableEvents), "Awake")]
[SupportedOSPlatform("windows")]
internal static class MenuSelectionAudioAwakePatch
{
    [HarmonyPostfix]
    private static void AfterAwake(SelectableEvents __instance) =>
        BopItAccessMod.NoteMenuSelectionAudioAwake(__instance);
}

[HarmonyPatch]
[SupportedOSPlatform("windows")]
internal static class MenuSelectionAudioCallbackPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (string method in new[] { "OnSelect", "OnDeselect", "OnPointerEnter",
                     "OnPointerExit", "ReturnToNormal", "Update" })
            yield return AccessTools.Method(typeof(SelectableEvents), method);
    }

    [HarmonyPrefix]
    private static void BeforeCallback(SelectableEvents __instance,
        MethodBase __originalMethod, out BopItAccessMod.MenuSelectionAudioScope? __state) =>
        __state = BopItAccessMod.BeginMenuSelectionAudioCallback(__instance,
            __originalMethod.Name);

    [HarmonyFinalizer]
    private static void AfterCallback(BopItAccessMod.MenuSelectionAudioScope? __state) =>
        BopItAccessMod.EndMenuSelectionAudioCallback(__state);
}

[HarmonyPatch(typeof(StudioEventEmitter), "Play")]
[SupportedOSPlatform("windows")]
internal static class MenuSelectionSoundPlaybackPatch
{
    [HarmonyPrefix]
    private static bool BeforePlay(StudioEventEmitter __instance) =>
        BopItAccessMod.AllowMenuSelectionSound(__instance);
}
