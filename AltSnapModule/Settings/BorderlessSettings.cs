using L2Presence.AltSnapModule.Input;

namespace L2Presence.AltSnapModule.Settings;

[Flags]
internal enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

internal sealed class BorderlessShortcut
{
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.Alt | HotkeyModifiers.Shift;
    public MouseButtonKind Button { get; set; } = MouseButtonKind.Middle;

    public BorderlessShortcut Clone() => new() { Modifiers = Modifiers, Button = Button };
}

internal sealed class BorderlessSettings
{
    public BorderlessShortcut ToggleShortcut { get; set; } = new();

    public BorderlessSettings Clone() => new()
    {
        ToggleShortcut = ToggleShortcut.Clone()
    };
}
