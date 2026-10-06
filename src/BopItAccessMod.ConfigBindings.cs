using System.Runtime.Versioning;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private readonly HashSet<int> _configNativeLoadedBindingManagers = new();
    private static readonly HashSet<string> ConfigPlayerActionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bop", "AltBop", "Twist", "Pull", "Spin", "Flick", "ResetGyro",
        "Navigate", "Move", "Submit", "Back", "Menu", "Pause",
        "Fullscreen", "ToggleFullscreen", "ChangeGroup", "ChangeDateRange",
        "ChangeDevice", "ChangeDeviceType", "ChangeDeviceFilter", "TurnPage",
        "NextPage", "PreviousPage", "ChangePage"
    };
    private static readonly string[] ConfigNativeDeviceProfiles =
        { "Keyboard", "Xbox", "PlayStation", "Generic Gamepad", "GenericGamepad", "Switch" };

    internal static void NoteConfigNativeBindingsLoaded(InputRebindingManager manager)
    {
        _activeNativeAudioMod?._configNativeLoadedBindingManagers.Add(manager.GetInstanceID());
    }

    // Match the native manager's Start gate: Steam/cloud preferences can
    // still be loading after the PlatformIO singleton has been created.
    private bool NativeConfigBindingsCanInitialize(InputRebindingManager manager)
    {
        if (manager.inputActions == null || manager.inputActions.actionMaps.Count == 0 ||
            PlatformIO.Instance?.IsInitialized != true)
            return false;
        // LoadBindings calls EnsureActiveDeviceIsSet itself. That method copies
        // the existing tracker's ActiveDevice, or finds the active scene tracker
        // through SetupDeviceTracker. Do not force-open Controls for setup.
        InputDeviceTracker? tracker = manager.deviceTracker ??
            UnityEngine.Object.FindFirstObjectByType<InputDeviceTracker>();
        return tracker != null && !string.IsNullOrWhiteSpace(tracker.ActiveDevice) &&
            HydrateConfigNativePlayerInput(manager);
    }

    private bool NativeConfigBindingsReady(InputRebindingManager manager) =>
        manager.inputActions != null && manager.inputActions.actionMaps.Count != 0 &&
        !string.IsNullOrWhiteSpace(manager.ActiveDevice) &&
        _configNativeLoadedBindingManagers.Contains(manager.GetInstanceID()) &&
        HydrateConfigNativePlayerInput(manager);

    private bool HydrateConfigNativePlayerInput(InputRebindingManager manager)
    {
        // The native Show method obtains this same App.PlayerInput reference.
        // LoadBindings and Start do not populate it, so a hidden Controls panel
        // otherwise cannot mirror configured changes to the active input copy.
        if (manager.playerInput == null)
        {
            MainMenuUIManager? main = _mainMenu ??
                UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            PlayerInput? playerInput = main?.app?.PlayerInput;
            if (playerInput == null)
                return false;
            manager.playerInput = playerInput;
        }
        return manager.playerInput.actions != null &&
            manager.playerInput.actions.actionMaps.Count != 0;
    }

    private void ApplyNativeConfigBindings(InputRebindingManager manager,
        IReadOnlyDictionary<string, string> values)
    {
        InputActionAsset asset = manager.inputActions;
        var currentValues = CaptureNativeConfigBindings(manager);
        var accepted = new List<(ConfigNativeBinding Binding, string? Path)>();
        string beforeJson = InputActionRebindingExtensions.SaveBindingOverridesAsJson(
            asset.Cast<IInputActionCollection2>());

        foreach (ConfigNativeBinding item in ConfigNativeBindingRows(asset))
        {
            if (!values.TryGetValue(item.Key, out string? text))
                continue;
            bool useDefault = text.Trim().Equals("Default", StringComparison.OrdinalIgnoreCase);
            if (!TryConfigNativeBindingPath(text, item, out string? path))
            {
                WriteStatus("Config ignored " + item.Key + ": use Default, None, or a valid input path for this device.");
                continue;
            }
            string wanted = useDefault ? item.Action.bindings[item.Index].path : path ?? string.Empty;
            if (currentValues.TryGetValue(item.Key, out string? current) &&
                string.Equals(current, wanted.Length == 0 ? "None" : wanted, StringComparison.OrdinalIgnoreCase))
                continue;
            // The game's defaults intentionally share controls across different
            // contexts (for example Bop and UI Submit). Restoring a default
            // must retain those native relationships. New mappings use the
            // existing duplicate guard, including all five mod-owned actions.
            if (!useDefault && !string.IsNullOrEmpty(path) &&
                (IsBindingAssignedElsewhere(manager, item.Action, item.Index, path, out string owner) ||
                 ConfigModHasLayoutConflict(manager, item.Action, item.Index, path, out owner)))
            {
                WriteStatus("Config ignored " + item.Key + ": input is already assigned to " + owner + ".");
                continue;
            }
            if (accepted.Any(candidate => !string.IsNullOrEmpty(path) &&
                    !string.IsNullOrEmpty(candidate.Path) &&
                    string.Equals(ConfigModConflictPath(candidate.Path), ConfigModConflictPath(path),
                        StringComparison.OrdinalIgnoreCase)))
            {
                WriteStatus("Config ignored " + item.Key + ": duplicates another requested mapping.");
                continue;
            }
            // Gameplay and leaderboard inputs keep the game's own reserved
            // controls/prompt checks. UI navigation itself contains reserved
            // menu controls and is validated by the known control path parser.
            string map = item.Action.actionMap.name;
            if (!useDefault && !string.IsNullOrEmpty(path) &&
                !string.Equals(map, "UI", StringComparison.OrdinalIgnoreCase) &&
                !manager.ValidateBinding(ControlNameFormatter.Format(path), path,
                    item.Action, out string reason))
            {
                WriteStatus("Config ignored " + item.Key + ": " + reason);
                continue;
            }
            RestoreNativeBindingOverride(item.Action, item.Index, path);
            InputAction? runtime = manager.playerInput?.actions?
                .FindActionMap(item.Action.actionMap.name, false)?.FindAction(item.Action.name, false);
            if (runtime != null && item.Index < runtime.bindings.Count)
                RestoreNativeBindingOverride(runtime, item.Index, path);
            accepted.Add((item, path));
        }
        if (accepted.Count == 0)
            return;

        // SaveBindings stores only the currently selected device profile.
        // Merge the changed IDs into other existing profiles as well, keeping
        // unrelated overrides (including profiles not currently connected).
        // This prevents switching devices from resurrecting the old mapping.
        manager.SaveBindings();
        string activeDevice = manager.ActiveDevice;
        foreach (string profile in ConfigNativeDeviceProfiles)
        {
            if (string.Equals(profile, activeDevice, StringComparison.OrdinalIgnoreCase))
                continue;
            string preference = "InputBindings_" + profile;
            string json = PlayerPrefs.HasKey(preference) ? PlayerPrefs.GetString(preference) : beforeJson;
            try
            {
                JsonObject document = JsonNode.Parse(json) as JsonObject ?? new JsonObject();
                JsonArray entries = document["bindings"] as JsonArray ?? new JsonArray();
                if (document["bindings"] == null)
                    document["bindings"] = entries;
                foreach (var change in accepted)
                    MergeConfigNativeOverride(entries, change.Binding, change.Path);
                PlayerPrefs.SetString(preference, document.ToJsonString());
            }
            catch (Exception ex)
            {
                // Never erase a profile whose native JSON cannot be parsed.
                WriteStatus("Could not merge configured bindings into " + profile + ": " + ex.Message);
            }
        }
        PlayerPrefs.Save();
        PlatformIO.Instance?.SavePlayerPrefs();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        _nativeBindingGuardSnapshot.Clear();
        WriteStatus("Applied " + accepted.Count + " configured game control mapping(s).");
    }

    private Dictionary<string, string> CaptureNativeConfigBindings(InputRebindingManager manager)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        InputActionAsset? asset = manager.inputActions;
        if (asset == null)
            return result;
        var profiles = new Dictionary<string, Dictionary<string, string?>>(StringComparer.OrdinalIgnoreCase);
        foreach (string profile in ConfigNativeDeviceProfiles)
        {
            string preference = "InputBindings_" + profile;
            if (!PlayerPrefs.HasKey(preference))
                continue;
            try
            {
                var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                JsonObject? document = JsonNode.Parse(PlayerPrefs.GetString(preference)) as JsonObject;
                if (document?["bindings"] is JsonArray entries)
                    foreach (JsonNode? node in entries)
                    {
                        string? id = node?["id"]?.GetValue<string>();
                        if (!string.IsNullOrEmpty(id))
                            overrides[id] = node?["path"]?.GetValue<string>();
                    }
                profiles[profile] = overrides;
            }
            catch (Exception ex)
            {
                WriteStatus("Could not read " + profile + " binding preferences for config: " + ex.Message);
            }
        }
        foreach (ConfigNativeBinding item in ConfigNativeBindingRows(asset))
        {
            InputBinding binding = item.Action.bindings[item.Index];
            string? path = binding.effectivePath;
            string profile = ConfigNativeProfileForLayout(item.Layout, manager.ActiveDevice);
            if (profiles.TryGetValue(profile, out var overrides))
                path = overrides.TryGetValue(binding.id.ToString(), out string? saved) && saved != null
                    ? saved : binding.path;
            result[item.Key] = string.IsNullOrEmpty(path) ? "None" : path;
        }
        return result;
    }

    private static IEnumerable<ConfigNativeBinding> ConfigNativeBindingRows(InputActionAsset asset)
    {
        foreach (InputActionMap map in asset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                if (!ConfigPlayerActionNames.Contains(action.name))
                    continue;
                int keyboard = 0;
                int controller = 0;
                for (int index = 0; index < action.bindings.Count; index++)
                {
                    InputBinding binding = action.bindings[index];
                    if (binding.isComposite || BindingIsModifierChordPart(action, index))
                        continue;
                    string? layout = HintDeviceLayout(binding.path);
                    bool isKeyboard = string.Equals(layout, "Keyboard", StringComparison.OrdinalIgnoreCase);
                    if (!isKeyboard && !IsHintControllerLayout(layout))
                        continue;
                    string suffix = isKeyboard ? "Keyboard" + (++keyboard) : "Controller" + (++controller);
                    if (binding.isPartOfComposite && !string.IsNullOrWhiteSpace(binding.name))
                        suffix += "." + binding.name;
                    yield return new ConfigNativeBinding("GameBindings." + map.name + "." + action.name + "." + suffix,
                        action, index, layout!, isKeyboard);
                }
            }
        }
    }

    private static bool TryConfigNativeBindingPath(string text, ConfigNativeBinding item,
        out string? path)
    {
        text = text.Trim();
        path = null;
        if (text.Equals("Default", StringComparison.OrdinalIgnoreCase))
            return true;
        if (text.Equals("None", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("Unbound", StringComparison.OrdinalIgnoreCase))
        {
            path = string.Empty;
            return true;
        }
        if (!text.StartsWith('<'))
        {
            string control = text.Replace(" ", string.Empty).Replace("-", string.Empty);
            if (item.IsKeyboard)
            {
                control = control.ToLowerInvariant() switch
                {
                    "left" => "leftArrow", "right" => "rightArrow", "up" => "upArrow", "down" => "downArrow",
                    "return" => "enter", "esc" => "escape", "ctrl" => "leftCtrl", _ => control
                };
            }
            else
            {
                control = control.ToLowerInvariant() switch
                {
                    "a" or "south" => "buttonSouth", "b" or "east" => "buttonEast",
                    "x" or "west" => "buttonWest", "y" or "north" => "buttonNorth",
                    "lt" => "leftTrigger", "rt" => "rightTrigger", "lb" => "leftShoulder", "rb" => "rightShoulder",
                    "start" or "menu" => "startButton", "select" or "back" or "view" => "selectButton",
                    "leftstickpress" or "ls" => "leftStickPress", "rightstickpress" or "rs" => "rightStickPress",
                    "dpadup" => "dpad/up", "dpaddown" => "dpad/down", "dpadleft" => "dpad/left", "dpadright" => "dpad/right",
                    _ => control
                };
            }
            text = "<" + item.Layout + ">/" + control;
        }
        Match match = Regex.Match(text, "^<([A-Za-z0-9_]+)>/([A-Za-z0-9_/]+)$");
        if (!match.Success)
            return false;
        string layout = match.Groups[1].Value;
        string controlName = match.Groups[2].Value;
        if (item.IsKeyboard)
        {
            if (!layout.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
                return false;
            // Share canonical key handling with mod bindings, including
            // Digit1 -> 1 and aliases on a connected keyboard.
            path = ConfigModBindingPath(text, controller: false, defaultPath: string.Empty);
            return path != null;
        }
        else
        {
            if (!layout.Equals(item.Layout, StringComparison.OrdinalIgnoreCase) &&
                !layout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
                return false;
            string[] valid = { "buttonSouth", "buttonEast", "buttonWest", "buttonNorth", "leftShoulder", "rightShoulder",
                "leftTrigger", "rightTrigger", "leftStick", "rightStick", "leftStickPress", "rightStickPress",
                "startButton", "selectButton", "dpad", "dpad/up", "dpad/down", "dpad/left", "dpad/right",
                "leftStick/up", "leftStick/down", "leftStick/left", "leftStick/right",
                "rightStick/up", "rightStick/down", "rightStick/left", "rightStick/right" };
            // Preserve controller-specific motion paths already supplied by
            // the game, while new mappings use known physical gamepad controls.
            if (!valid.Contains(controlName, StringComparer.OrdinalIgnoreCase) &&
                !string.Equals(text, item.Action.bindings[item.Index].path, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        path = text;
        return true;
    }

    private static string ConfigNativeProfileForLayout(string layout, string activeDevice)
    {
        if (layout.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return "Keyboard";
        if (layout.Contains("XInput", StringComparison.OrdinalIgnoreCase) || layout.Contains("Xbox", StringComparison.OrdinalIgnoreCase))
            return "Xbox";
        if (layout.Contains("Dual", StringComparison.OrdinalIgnoreCase) || layout.Contains("PlayStation", StringComparison.OrdinalIgnoreCase))
            return "PlayStation";
        if (layout.Contains("Switch", StringComparison.OrdinalIgnoreCase) || layout.Contains("NPad", StringComparison.OrdinalIgnoreCase))
            return "Switch";
        return IsGamepadDevice(activeDevice) ? activeDevice :
            PlayerPrefs.HasKey("InputBindings_GenericGamepad") ? "GenericGamepad" : "Generic Gamepad";
    }

    private static void MergeConfigNativeOverride(JsonArray entries, ConfigNativeBinding item, string? path)
    {
        string id = item.Action.bindings[item.Index].id.ToString();
        JsonObject? entry = entries.OfType<JsonObject>().FirstOrDefault(candidate =>
            string.Equals(candidate["id"]?.GetValue<string>(), id, StringComparison.OrdinalIgnoreCase));
        if (path == null)
        {
            if (entry != null)
                entries.Remove(entry);
            return;
        }
        if (entry == null)
        {
            entry = new JsonObject
            {
                ["action"] = item.Action.actionMap.name + "/" + item.Action.name,
                ["id"] = id,
                ["interactions"] = null,
                ["processors"] = null
            };
            entries.Add(entry);
        }
        entry["path"] = path;
    }

    private sealed record ConfigNativeBinding(string Key, InputAction Action,
        int Index, string Layout, bool IsKeyboard);
}

[HarmonyPatch(typeof(InputRebindingManager), "LoadBindings")]
[SupportedOSPlatform("windows")]
internal static class ConfigNativeBindingsLoadPatch
{
    [HarmonyPostfix]
    private static void AfterLoad(InputRebindingManager __instance)
    {
        try { BopItAccessMod.NoteConfigNativeBindingsLoaded(__instance); }
        catch { /* Tracking config readiness must not break native binding restoration. */ }
    }
}


[HarmonyPatch(typeof(InputRebindingManager), "SaveBindings")]
[SupportedOSPlatform("windows")]
internal static class ConfigNativeBindingsSavePatch
{
    [HarmonyPostfix]
    private static void AfterSave()
    {
        try { BopItAccessMod.NoteSettingsConfigNativeSaved(); }
        catch { /* Config synchronization must not break native binding persistence. */ }
    }
}
