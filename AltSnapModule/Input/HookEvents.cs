using System.Drawing;

namespace L2Presence.AltSnapModule.Input;

internal enum MouseButtonKind
{
    Left,
    Right,
    Middle,
    XButton1,
    XButton2
}

internal sealed record KeyboardHookEvent(int VirtualKey, bool IsDown);
internal sealed record MouseButtonHookEvent(MouseButtonKind Button, bool IsDown, Point ScreenPoint);
