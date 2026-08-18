using System.Diagnostics;

namespace L2Presence;

internal static class L2WindowDiagnostics
{
    public static void Print(AppSettings settings)
    {
        var processes = Process.GetProcessesByName(settings.ProcessName)
            .OrderBy(p => p.Id)
            .ToArray();

        try
        {
            Console.WriteLine("=== L2 WINDOW DIAGNOSTICS ===");
            Console.WriteLine($"Foreground HWND: 0x{NativeMethods.GetForegroundWindowHandle().ToInt64():X}");
            Console.WriteLine($"Foreground PID : {NativeMethods.GetForegroundProcessId()?.ToString() ?? "n/a"}");
            Console.WriteLine($"Found {processes.Length} {settings.ProcessName}.exe process(es).\n");

            if (processes.Length == 0)
            {
                Console.WriteLine("No L2 processes found.");
                return;
            }

            foreach (var process in processes)
            {
                Console.WriteLine($"--- PID {process.Id} ---");
                Console.WriteLine($"Process.MainWindowHandle: 0x{process.MainWindowHandle.ToInt64():X}");
                Console.WriteLine($"Process.MainWindowTitle : {Quote(process.MainWindowTitle)}");

                var windows = NativeMethods.EnumerateWindowsForProcess(process.Id, includeChildren: true);
                if (windows.Count == 0)
                {
                    Console.WriteLine("No Win32 windows found.");
                    Console.WriteLine();
                    continue;
                }

                foreach (var window in windows)
                {
                    var kind = window.IsChild ? "CHILD" : "TOP  ";
                    var marker = window.Handle == process.MainWindowHandle ? " MAIN" : string.Empty;
                    Console.WriteLine(
                        $"[{kind}] HWND=0x{window.Handle.ToInt64():X} PID={window.ProcessId} " +
                        $"Visible={window.IsVisible,-5}{marker} Class={Quote(window.ClassName)} Title={Quote(window.Title)}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Copy everything above and send it back to me.");
        }
        finally
        {
            foreach (var process in processes)
                process.Dispose();
        }
    }

    private static string Quote(string? value)
        => string.IsNullOrEmpty(value) ? "<empty>" : $"\"{value}\"";
}
