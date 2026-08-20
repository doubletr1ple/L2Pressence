using L2Presence.AltSnapModule.Interop;
using L2Presence.AltSnapModule.Settings;

namespace L2Presence.AltSnapModule.Input;

internal sealed class ModifierState
{
    private readonly object _gate = new();
    private readonly HashSet<int> _downKeys = new();

    public void ResetFromSystem()
    {
        lock (_gate)
        {
            _downKeys.Clear();
            AddIfDown(Win32.VkLMenu);
            AddIfDown(Win32.VkRMenu);
            AddIfDown(Win32.VkLControl);
            AddIfDown(Win32.VkRControl);
            AddIfDown(Win32.VkLShift);
            AddIfDown(Win32.VkRShift);
            AddIfDown(Win32.VkLWin);
            AddIfDown(Win32.VkRWin);
        }
    }

    public bool Update(int virtualKey, bool down)
    {
        lock (_gate)
        {
            if (!IsModifierKey(virtualKey))
                return false;

            if (down)
                _downKeys.Add(virtualKey);
            else
                _downKeys.Remove(virtualKey);
            return true;
        }
    }

    public HotkeyModifiers Snapshot()
    {
        lock (_gate)
        {
            var modifiers = HotkeyModifiers.None;
            if (HasAny(Win32.VkMenu, Win32.VkLMenu, Win32.VkRMenu)) modifiers |= HotkeyModifiers.Alt;
            if (HasAny(Win32.VkControl, Win32.VkLControl, Win32.VkRControl)) modifiers |= HotkeyModifiers.Control;
            if (HasAny(Win32.VkShift, Win32.VkLShift, Win32.VkRShift)) modifiers |= HotkeyModifiers.Shift;
            if (HasAny(Win32.VkLWin, Win32.VkRWin)) modifiers |= HotkeyModifiers.Windows;
            return modifiers;
        }
    }

    private void AddIfDown(int virtualKey)
    {
        if (Win32.IsKeyDown(virtualKey))
            _downKeys.Add(virtualKey);
    }

    private bool HasAny(params int[] keys) => keys.Any(_downKeys.Contains);

    private static bool IsModifierKey(int virtualKey) => virtualKey is
        Win32.VkMenu or Win32.VkLMenu or Win32.VkRMenu or
        Win32.VkControl or Win32.VkLControl or Win32.VkRControl or
        Win32.VkShift or Win32.VkLShift or Win32.VkRShift or
        Win32.VkLWin or Win32.VkRWin;
}
