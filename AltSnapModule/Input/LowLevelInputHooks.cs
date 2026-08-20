using System.Runtime.InteropServices;
using L2Presence.AltSnapModule.Interop;

namespace L2Presence.AltSnapModule.Input;

internal sealed class LowLevelInputHooks : IDisposable
{
    private readonly Win32.HookProc _keyboardProc;
    private readonly Win32.HookProc _mouseProc;
    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;

    public event Func<KeyboardHookEvent, bool>? Keyboard;
    public event Func<MouseButtonHookEvent, bool>? MouseButton;

    public bool IsInstalled => _keyboardHook != IntPtr.Zero && _mouseHook != IntPtr.Zero;

    public LowLevelInputHooks()
    {
        _keyboardProc = KeyboardCallback;
        _mouseProc = MouseCallback;
    }

    public void Install()
    {
        if (IsInstalled)
            return;

        _keyboardHook = Win32.SetWindowsHookEx(Win32.WhKeyboardLl, _keyboardProc, IntPtr.Zero, 0);
        _mouseHook = Win32.SetWindowsHookEx(Win32.WhMouseLl, _mouseProc, IntPtr.Zero, 0);

        if (_keyboardHook == IntPtr.Zero || _mouseHook == IntPtr.Zero)
        {
            Dispose();
            throw new InvalidOperationException("Could not install borderless input hooks.");
        }
    }

    public void Dispose()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            Win32.UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }

        if (_mouseHook != IntPtr.Zero)
        {
            Win32.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }

    private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();
            var isDown = message is Win32.WmKeyDown or Win32.WmSysKeyDown;
            var isUp = message is Win32.WmKeyUp or Win32.WmSysKeyUp;

            if (isDown || isUp)
            {
                var data = Marshal.PtrToStructure<KeyboardHookStruct>(lParam);
                if (Keyboard?.Invoke(new KeyboardHookEvent(data.VkCode, isDown)) == true)
                    return 1;
            }
        }

        return Win32.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();
            var data = Marshal.PtrToStructure<MouseHookStruct>(lParam);
            var point = data.Point.ToPoint();

            if (TryCreateButtonEvent(message, data.MouseData, point, out var buttonEvent) &&
                     MouseButton?.Invoke(buttonEvent) == true)
            {
                return 1;
            }
        }

        return Win32.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    private static bool TryCreateButtonEvent(
        int message,
        int mouseData,
        System.Drawing.Point point,
        out MouseButtonHookEvent buttonEvent)
    {
        var xButton = (mouseData >> 16) & 0xffff;
        buttonEvent = message switch
        {
            Win32.WmLButtonDown => new MouseButtonHookEvent(MouseButtonKind.Left, true, point),
            Win32.WmLButtonUp => new MouseButtonHookEvent(MouseButtonKind.Left, false, point),
            Win32.WmRButtonDown => new MouseButtonHookEvent(MouseButtonKind.Right, true, point),
            Win32.WmRButtonUp => new MouseButtonHookEvent(MouseButtonKind.Right, false, point),
            Win32.WmMButtonDown => new MouseButtonHookEvent(MouseButtonKind.Middle, true, point),
            Win32.WmMButtonUp => new MouseButtonHookEvent(MouseButtonKind.Middle, false, point),
            Win32.WmXButtonDown when xButton == 1 => new MouseButtonHookEvent(MouseButtonKind.XButton1, true, point),
            Win32.WmXButtonUp when xButton == 1 => new MouseButtonHookEvent(MouseButtonKind.XButton1, false, point),
            Win32.WmXButtonDown when xButton == 2 => new MouseButtonHookEvent(MouseButtonKind.XButton2, true, point),
            Win32.WmXButtonUp when xButton == 2 => new MouseButtonHookEvent(MouseButtonKind.XButton2, false, point),
            _ => null!
        };

        return buttonEvent is not null;
    }
}
