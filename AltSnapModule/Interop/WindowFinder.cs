using System.Drawing;

namespace L2Presence.AltSnapModule.Interop;

internal static class WindowFinder
{
    public static IntPtr GetRootWindowFromPoint(Point point)
    {
        var child = Win32.WindowFromPoint(new NativePoint { X = point.X, Y = point.Y });
        if (child == IntPtr.Zero)
            return IntPtr.Zero;

        var root = Win32.GetAncestor(child, Win32.GaRoot);
        if (root == IntPtr.Zero || !Win32.IsWindow(root) || !Win32.IsWindowVisible(root))
            return IntPtr.Zero;

        return root;
    }
}
