using L2Presence.AltSnapModule.Interop;

namespace L2Presence.AltSnapModule.Actions;

internal sealed class BorderlessWindowStore
{
    private const WindowStyles FrameStyles =
        WindowStyles.Caption |
        WindowStyles.ThickFrame |
        WindowStyles.SystemMenu |
        WindowStyles.MinimizeBox |
        WindowStyles.MaximizeBox;

    private const ExtendedWindowStyles FrameExtendedStyles =
        ExtendedWindowStyles.DialogModalFrame |
        ExtendedWindowStyles.WindowEdge |
        ExtendedWindowStyles.ClientEdge |
        ExtendedWindowStyles.StaticEdge;

    private readonly Dictionary<IntPtr, WindowStyleSnapshot> _originalStyles = new();

    public bool Toggle(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return false;

        var currentStyles = ReadStyles(hWnd);
        if (_originalStyles.TryGetValue(hWnd, out var originalStyles))
        {
            var expectedBorderlessStyles = RemoveFrame(originalStyles);
            if (HasFrame(currentStyles))
            {
                ApplyStyles(hWnd, expectedBorderlessStyles);
                return true;
            }

            ApplyStyles(hWnd, originalStyles);
            _originalStyles.Remove(hWnd);
            return false;
        }

        _originalStyles[hWnd] = currentStyles;
        ApplyStyles(hWnd, RemoveFrame(currentStyles));
        return true;
    }

    public void RestoreAll()
    {
        foreach (var (hWnd, styles) in _originalStyles.ToArray())
        {
            if (!Win32.IsWindow(hWnd))
                continue;

            ApplyStyles(hWnd, styles);
        }

        _originalStyles.Clear();
    }

    private static WindowStyleSnapshot ReadStyles(IntPtr hWnd)
        => new(
            Win32.GetWindowLongPtr(hWnd, Win32.GwlStyle),
            Win32.GetWindowLongPtr(hWnd, Win32.GwlExStyle));

    private static WindowStyleSnapshot RemoveFrame(WindowStyleSnapshot styles)
        => new(
            new IntPtr(styles.Style.ToInt64() & ~(long)FrameStyles),
            new IntPtr(styles.ExtendedStyle.ToInt64() & ~(long)FrameExtendedStyles));

    private static bool HasFrame(WindowStyleSnapshot styles)
        => (styles.Style.ToInt64() & (long)FrameStyles) != 0 ||
           (styles.ExtendedStyle.ToInt64() & (long)FrameExtendedStyles) != 0;

    private static void ApplyStyles(IntPtr hWnd, WindowStyleSnapshot styles)
    {
        if (!Win32.GetWindowRect(hWnd, out var windowRect))
        {
            throw new System.ComponentModel.Win32Exception(
                System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                "Windows could not read the target window bounds.");
        }

        Win32.SetWindowLongPtrChecked(hWnd, Win32.GwlStyle, styles.Style);
        Win32.SetWindowLongPtrChecked(hWnd, Win32.GwlExStyle, styles.ExtendedStyle);
        if (!Win32.SetWindowPos(
                hWnd,
                IntPtr.Zero,
                windowRect.Left,
                windowRect.Top,
                windowRect.Width,
                windowRect.Height,
                SetWindowPosFlags.NoZOrder |
                SetWindowPosFlags.NoActivate |
                SetWindowPosFlags.FrameChanged))
        {
            throw new System.ComponentModel.Win32Exception(
                System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                "Windows could not redraw the borderless frame.");
        }

        NotifyClientSizeChanged(hWnd);

        var actualStyles = ReadStyles(hWnd);
        if (HasFrame(actualStyles) != HasFrame(styles))
            throw new InvalidOperationException("The target window restored its frame immediately.");
    }

    private static void NotifyClientSizeChanged(IntPtr hWnd)
    {
        if (!Win32.GetClientRect(hWnd, out var clientRect))
        {
            throw new System.ComponentModel.Win32Exception(
                System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                "Windows could not read the new client area.");
        }

        if (!Win32.PostMessage(
                hWnd,
                Win32.WmSize,
                new IntPtr(Win32.SizeRestored),
                Win32.MakeSizeParameter(clientRect.Width, clientRect.Height)))
        {
            throw new System.ComponentModel.Win32Exception(
                System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                "Windows could not notify the target window about its new client size.");
        }
    }

    private readonly record struct WindowStyleSnapshot(IntPtr Style, IntPtr ExtendedStyle);
}
