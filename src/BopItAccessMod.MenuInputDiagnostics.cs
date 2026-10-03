using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private sealed class PendingMenuInputDiagnostic
    {
        internal string Input = string.Empty;
        internal string Menu = string.Empty;
        internal string SelectedName = string.Empty;
        internal int SelectedId;
        internal int LateUpdatesRemaining = 2;
    }

    private readonly List<PendingMenuInputDiagnostic> _pendingMenuInputDiagnostics = new();
    private readonly HashSet<string> _seenMenuInputControlPaths =
        new(StringComparer.OrdinalIgnoreCase);
    private MainMenuUIManager? _menuInputDiagnosticMain;
    private long _nextMenuInputDiagnosticMainSearchAt;
    private long _nextMenuInputDiagnosticErrorAt;
    private int _lastMenuInputGamepadSignature = int.MinValue;

    // Diagnostic observations only: the game's UI module still owns navigation.
    // This runs after Input System updates, before the EventSystem's resulting
    // selection is checked in OnLateUpdate.
    private void BeginMenuInputDiagnosticFrame()
    {
        if (!Application.isFocused)
            return;

        try
        {
            if (!TryGetNativeMenuForInputDiagnostic(out string menu))
            {
                _lastMenuInputGamepadSignature = int.MinValue;
                return;
            }

            LogMenuInputGamepadInventoryIfChanged();
            InputSystemUIInputModule? module =
                EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            InputAction? navigate = module?.move?.action;
            if (navigate == null)
                navigate = FindHintAction("UI", "Navigate");

            _seenMenuInputControlPaths.Clear();
            HashSet<string> seen = _seenMenuInputControlPaths;
            foreach (Gamepad gamepad in Gamepad.all)
            {
                CaptureMenuInputEdge(gamepad.dpad.up, "D-pad up", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.dpad.down, "D-pad down", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.dpad.left, "D-pad left", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.dpad.right, "D-pad right", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.leftStick.up, "left stick up", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.leftStick.down, "left stick down", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.leftStick.left, "left stick left", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.leftStick.right, "left stick right", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.rightStick.up, "right stick up", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.rightStick.down, "right stick down", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.rightStick.left, "right stick left", menu,
                    navigate, seen);
                CaptureMenuInputEdge(gamepad.rightStick.right, "right stick right", menu,
                    navigate, seen);
            }

            // Steam Input may present a controller as keyboard keys, while
            // other devices may appear as generic Joysticks. Include button
            // controls from the live UI action so both cases and remapped
            // controls are observed. The seen set avoids duplicate D-pad
            // observations from the explicit Gamepad scan above.
            if (navigate != null)
                foreach (InputControl control in navigate.controls)
                    if (control.device is Keyboard or Gamepad or Joystick &&
                        control is ButtonControl button)
                        CaptureMenuInputEdge(button, control.name, menu,
                            navigate, seen);

            Keyboard? keyboard = Keyboard.current;
            if (keyboard != null)
            {
                CaptureMenuInputEdge(keyboard.upArrowKey, "Up arrow", menu,
                    navigate, seen);
                CaptureMenuInputEdge(keyboard.downArrowKey, "Down arrow", menu,
                    navigate, seen);
                CaptureMenuInputEdge(keyboard.wKey, "W", menu,
                    navigate, seen);
                CaptureMenuInputEdge(keyboard.sKey, "S", menu,
                    navigate, seen);
            }
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextMenuInputDiagnosticErrorAt)
            {
                WriteStatus("Menu input diagnostic failed: " + ex.Message);
                _nextMenuInputDiagnosticErrorAt = now + 5000;
            }
        }
    }

    private void FinishMenuInputDiagnosticFrame()
    {
        if (!Application.isFocused)
        {
            _pendingMenuInputDiagnostics.Clear();
            return;
        }
        if (_pendingMenuInputDiagnostics.Count == 0)
            return;

        try
        {
            string menu = TryGetNativeMenuForInputDiagnostic(out string currentMenu)
                ? currentMenu : "another screen";
            GameObject? selected = EventSystem.current?.currentSelectedGameObject;
            int selectedId = selected == null ? 0 : selected.GetInstanceID();
            string selectedName = selected == null ? "none" : selected.name;
            for (int i = _pendingMenuInputDiagnostics.Count - 1; i >= 0; i--)
            {
                PendingMenuInputDiagnostic pending =
                    _pendingMenuInputDiagnostics[i];
                if (selectedId != pending.SelectedId ||
                    !string.Equals(menu, pending.Menu, StringComparison.Ordinal))
                {
                    WriteStatus("Menu input diagnostic result: " + pending.Input +
                        "; selection changed from " + pending.Menu + "/" +
                        pending.SelectedName + " to " + menu + "/" +
                        selectedName + ".");
                    _pendingMenuInputDiagnostics.RemoveAt(i);
                }
                else if (--pending.LateUpdatesRemaining <= 0)
                {
                    // Staying on a boundary item may be intentional; this
                    // observation alone does not mean the game lost input.
                    WriteStatus("Menu input diagnostic result: " + pending.Input +
                        "; selection remained on " + menu + "/" +
                        selectedName + " after two frames.");
                    _pendingMenuInputDiagnostics.RemoveAt(i);
                }
            }
        }
        catch (Exception ex)
        {
            _pendingMenuInputDiagnostics.Clear();
            long now = Environment.TickCount64;
            if (now >= _nextMenuInputDiagnosticErrorAt)
            {
                WriteStatus("Menu input diagnostic result failed: " + ex.Message);
                _nextMenuInputDiagnosticErrorAt = now + 5000;
            }
        }
    }

    private bool TryGetNativeMenuForInputDiagnostic(out string menu)
    {
        menu = string.Empty;
        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = _menuInputDiagnosticMain;
        if (main == null)
        {
            long now = Environment.TickCount64;
            if (now < _nextMenuInputDiagnosticMainSearchAt)
                return false;
            _nextMenuInputDiagnosticMainSearchAt = now + 500;
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            _menuInputDiagnosticMain = main;
        }

        if (main?.panels == null || main.panels.Count == 0)
            return false;
        Panel top = main.panels.Peek();
        if (top == null)
            return false;
        int topId = top.GetInstanceID();
        if (main.mainMenuPanel != null &&
            topId == main.mainMenuPanel.GetInstanceID() &&
            IsHintPanelVisible(main.mainMenuPanel))
            menu = "Main menu";
        else if (main.gameModePanel != null &&
            topId == main.gameModePanel.GetInstanceID() &&
            IsHintPanelVisible(main.gameModePanel))
            menu = "Play modes";
        else if (main.settingsPanel != null &&
            topId == main.settingsPanel.GetInstanceID() &&
            IsHintPanelVisible(main.settingsPanel))
            menu = "Settings";
        return menu.Length != 0;
    }

    private void LogMenuInputGamepadInventoryIfChanged()
    {
        int signature = HashCode.Combine(Gamepad.all.Count, Joystick.all.Count);
        foreach (Gamepad gamepad in Gamepad.all)
            signature = HashCode.Combine(signature, gamepad.deviceId,
                gamepad.layout);
        foreach (Joystick joystick in Joystick.all)
            signature = HashCode.Combine(signature, joystick.deviceId,
                joystick.layout);
        if (signature == _lastMenuInputGamepadSignature)
            return;
        _lastMenuInputGamepadSignature = signature;
        var devices = new List<string>();
        foreach (Gamepad gamepad in Gamepad.all)
            devices.Add("Gamepad " + gamepad.deviceId + ":" + gamepad.layout);
        foreach (Joystick joystick in Joystick.all)
            devices.Add("Joystick " + joystick.deviceId + ":" + joystick.layout);
        WriteStatus("Menu input diagnostic: Unity sees " +
            Gamepad.all.Count + " gamepad(s) and " + Joystick.all.Count +
            " joystick(s)" +
            (devices.Count == 0 ? "." : " [" +
                string.Join(", ", devices) + "]."));
    }

    private void CaptureMenuInputEdge(ButtonControl? control, string label,
        string menu, InputAction? navigate, HashSet<string> seen)
    {
        if (control?.wasPressedThisFrame != true || !seen.Add(control.path))
            return;
        InputDevice device = control.device;
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        string selectedName = selected == null ? "none" : selected.name;
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        bool enabled = navigate?.enabled == true;
        Vector2 value = enabled ? navigate!.ReadValue<Vector2>() : Vector2.zero;
        string input = label + " [" + device.layout + " #" + device.deviceId +
            ", " + control.path + "]";
        WriteStatus("Menu input diagnostic edge: " + input + "; menu=" +
            menu + "; UI Navigate " + (enabled ? "enabled" : "disabled") +
            ", value=(" + value.x.ToString("0.00") + "," +
            value.y.ToString("0.00") + "); selected=" + selectedName + ".");
        if (_pendingMenuInputDiagnostics.Count < 16)
            _pendingMenuInputDiagnostics.Add(new PendingMenuInputDiagnostic
            {
                Input = input,
                Menu = menu,
                SelectedName = selectedName,
                SelectedId = selectedId
            });
    }
}
