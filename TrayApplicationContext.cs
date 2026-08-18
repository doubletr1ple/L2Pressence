using System.Drawing;
using System.Windows.Forms;

namespace L2Presence;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly L2WindowDetector _detector;
    private readonly DiscordPresenceService _discord;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly System.Windows.Forms.Timer _pollTimer;

    private string? _lastSignature;

    public TrayApplicationContext(AppSettings settings)
    {
        _detector = new L2WindowDetector(settings);
        _discord = new DiscordPresenceService(settings);
        _discord.Initialize();

        _statusItem = new ToolStripMenuItem("Status: waiting for Lineage II")
        {
            Enabled = false
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitThread();

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "L2Presence",
            ContextMenuStrip = menu,
            Visible = true
        };

        _pollTimer = new System.Windows.Forms.Timer
        {
            Interval = Math.Max(1, settings.PollIntervalSeconds) * 1000
        };
        _pollTimer.Tick += (_, _) => RefreshPresence();
        _pollTimer.Start();

        RefreshPresence();
    }

    private void RefreshPresence()
    {
        var windows = _detector.DetectAll();
        var signature = string.Join("|", windows.Select(x => $"{x.ProcessId}:{x.WindowTitle}"));

        if (string.Equals(_lastSignature, signature, StringComparison.Ordinal))
            return;

        _discord.UpdateCharacters(windows);
        _lastSignature = signature;

        UpdateTrayStatus(windows);
    }

    private void UpdateTrayStatus(IReadOnlyList<L2WindowInfo> windows)
    {
        var characters = windows
            .Select(x => x.CharacterName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (characters.Count == 0)
        {
            _statusItem.Text = "Status: Lineage II not detected";
            _notifyIcon.Text = "L2Presence — Lineage II not detected";
            return;
        }

        var names = string.Join(", ", characters);
        _statusItem.Text = characters.Count == 1
            ? $"Status: {characters[0]}"
            : $"Status: {characters.Count} clients — {names}";

        var tooltip = characters.Count == 1
            ? $"L2Presence — {characters[0]}"
            : $"L2Presence — {characters.Count} clients: {names}";

        // NotifyIcon.Text is limited to 63 characters on Windows.
        _notifyIcon.Text = tooltip.Length <= 63
            ? tooltip
            : tooltip[..60] + "...";
    }

    protected override void ExitThreadCore()
    {
        _pollTimer.Stop();
        _pollTimer.Dispose();

        _discord.Dispose();

        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();

        base.ExitThreadCore();
    }
}
