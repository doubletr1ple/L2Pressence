using System.Runtime.InteropServices;
using System.Text;

namespace L2Presence;

internal sealed record NativeWindowInfo(
    IntPtr Handle,
    int ProcessId,
    string Title,
    string ClassName,
    bool IsVisible,
    bool IsChild);

internal static class NativeMethods
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    public static IntPtr GetForegroundWindowHandle() => GetForegroundWindow();

    public static int? GetForegroundProcessId()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return null;

        GetWindowThreadProcessId(hwnd, out var processId);
        return processId == 0 ? null : checked((int)processId);
    }

    public static IReadOnlyList<NativeWindowInfo> EnumerateWindowsForProcess(int processId, bool includeChildren = true)
    {
        var result = new List<NativeWindowInfo>();
        var topLevelHandles = new List<IntPtr>();

        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out var pid);
            if (pid == processId)
            {
                topLevelHandles.Add(hWnd);
                result.Add(CreateWindowInfo(hWnd, checked((int)pid), isChild: false));
            }

            return true;
        }, IntPtr.Zero);

        if (includeChildren)
        {
            var seen = new HashSet<IntPtr>();
            foreach (var parent in topLevelHandles)
            {
                EnumChildWindows(parent, (hWnd, _) =>
                {
                    if (!seen.Add(hWnd))
                        return true;

                    GetWindowThreadProcessId(hWnd, out var pid);
                    result.Add(CreateWindowInfo(hWnd, checked((int)pid), isChild: true));
                    return true;
                }, IntPtr.Zero);
            }
        }

        return result;
    }

    private static NativeWindowInfo CreateWindowInfo(IntPtr hWnd, int processId, bool isChild)
        => new(
            hWnd,
            processId,
            ReadWindowText(hWnd),
            ReadClassName(hWnd),
            IsWindowVisible(hWnd),
            isChild);

    private static string ReadWindowText(IntPtr hWnd)
    {
        var length = GetWindowTextLength(hWnd);
        if (length <= 0)
            return string.Empty;

        var buffer = new StringBuilder(length + 1);
        GetWindowText(hWnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string ReadClassName(IntPtr hWnd)
    {
        var buffer = new StringBuilder(512);
        return GetClassName(hWnd, buffer, buffer.Capacity) > 0
            ? buffer.ToString()
            : string.Empty;
    }
}
