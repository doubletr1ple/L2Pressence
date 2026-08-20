using System.Runtime.InteropServices;

namespace L2Presence.AltSnapModule.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint
{
    public int X;
    public int Y;

    public readonly System.Drawing.Point ToPoint() => new(X, Y);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MouseHookStruct
{
    public NativePoint Point;
    public int MouseData;
    public int Flags;
    public int Time;
    public IntPtr ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct KeyboardHookStruct
{
    public int VkCode;
    public int ScanCode;
    public int Flags;
    public int Time;
    public IntPtr ExtraInfo;
}

[Flags]
internal enum WindowStyles : long
{
    Caption = 0x00C00000L,
    ThickFrame = 0x00040000L,
    SystemMenu = 0x00080000L,
    MinimizeBox = 0x00020000L,
    MaximizeBox = 0x00010000L,
}

[Flags]
internal enum ExtendedWindowStyles : long
{
    DialogModalFrame = 0x00000001L,
    WindowEdge = 0x00000100L,
    ClientEdge = 0x00000200L,
    StaticEdge = 0x00020000L,
}

[Flags]
internal enum SetWindowPosFlags : uint
{
    NoSize = 0x0001,
    NoMove = 0x0002,
    NoZOrder = 0x0004,
    NoActivate = 0x0010,
    FrameChanged = 0x0020,
}
