using System.Drawing;
using System.Windows.Forms;
using L2Presence.AltSnapModule;
using L2Presence.AltSnapModule.Interop;
using L2Presence.AltSnapModule.Settings;

namespace L2Presence;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly L2WindowDetector _detector;
    private readonly DiscordPresenceService _discord;
    private readonly Control _uiThreadControl;
    private readonly BorderlessSettingsStore _borderlessSettingsStore;
    private readonly BorderlessHotkeyService _borderless;
    private readonly Icon _trayIcon;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _borderlessToggleItem;
    private readonly ToolStripMenuItem _borderlessStatusItem;
    private readonly ToolStripMenuItem _borderlessShortcutItem;
    private readonly System.Windows.Forms.Timer _pollTimer;

    private string? _lastSignature;
    private BorderlessSettings _borderlessSettings;
    private IntPtr _trayTargetWindow;

    public TrayApplicationContext(AppSettings settings)
    {
        _detector = new L2WindowDetector(settings);
        _discord = new DiscordPresenceService(settings);
        _discord.Initialize();

        _uiThreadControl = new Control();
        _uiThreadControl.CreateControl();

        _borderlessSettingsStore = new BorderlessSettingsStore();
        _borderlessSettings = _borderlessSettingsStore.Load();
        _borderless = new BorderlessHotkeyService(_borderlessSettings);
        _borderless.StatusChanged += (_, _) => RunOnUiThread(UpdateBorderlessStatus);

        _statusItem = new ToolStripMenuItem("Status: waiting for Lineage II")
        {
            Enabled = false
        };

        _borderlessStatusItem = new ToolStripMenuItem(_borderless.Status)
        {
            Enabled = false
        };

        _borderlessToggleItem = new ToolStripMenuItem("Enable borderless hotkey")
        {
            CheckOnClick = true,
            Checked = false
        };
        _borderlessToggleItem.Click += (_, _) => ToggleBorderlessHotkey();

        var shortcutSettingsItem = new ToolStripMenuItem("Configure hotkey...");
        shortcutSettingsItem.Click += (_, _) => ShowBorderlessSettings();

        _borderlessShortcutItem = new ToolStripMenuItem { Enabled = false };

        var toggleBorderlessItem = new ToolStripMenuItem("Toggle borderless for active window");
        toggleBorderlessItem.Click += (_, _) => ToggleBorderlessForTrayTarget();

        var restoreBordersItem = new ToolStripMenuItem("Restore all window borders");
        restoreBordersItem.Click += (_, _) => _borderless.RestoreAllBorders();

        var borderlessMenu = new ToolStripMenuItem("Borderless");
        borderlessMenu.DropDownItems.Add(_borderlessStatusItem);
        borderlessMenu.DropDownItems.Add(_borderlessToggleItem);
        borderlessMenu.DropDownItems.Add(new ToolStripSeparator());
        borderlessMenu.DropDownItems.Add(shortcutSettingsItem);
        borderlessMenu.DropDownItems.Add(_borderlessShortcutItem);
        borderlessMenu.DropDownItems.Add(new ToolStripSeparator());
        borderlessMenu.DropDownItems.Add(toggleBorderlessItem);
        borderlessMenu.DropDownItems.Add(restoreBordersItem);

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitThread();

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(borderlessMenu);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);
        menu.Opening += (_, _) => _trayTargetWindow = Win32.GetForegroundWindow();

        _trayIcon = TrayIconFactory.Create();
        _notifyIcon = new NotifyIcon
        {
            Icon = _trayIcon,
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
        UpdateShortcutLabel();
    }

    private void ToggleBorderlessHotkey()
    {
        try
        {
            if (_borderlessToggleItem.Checked)
                _borderless.Start();
            else
                _borderless.Stop();
        }
        catch (Exception ex)
        {
            _borderlessToggleItem.Checked = false;
            MessageBox.Show(
                $"Borderless hotkey could not start.\n\n{ex.Message}",
                "L2Presence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        UpdateBorderlessStatus();
    }

    private void UpdateBorderlessStatus()
    {
        _borderlessStatusItem.Text = _borderless.LastError is null
            ? _borderless.Status
            : $"Borderless error: {_borderless.LastError}";
        _borderlessToggleItem.Text = _borderless.Enabled
            ? "Disable borderless hotkey"
            : "Enable borderless hotkey";
        _borderlessToggleItem.Checked = _borderless.Enabled;
    }

    private void ShowBorderlessSettings()
    {
        using var form = new BorderlessSettingsForm(_borderlessSettings);
        if (form.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            _borderlessSettingsStore.Save(form.Settings);
            _borderlessSettings = form.Settings;
            _borderless.ApplySettings(_borderlessSettings);
            UpdateShortcutLabel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Borderless settings could not be saved.\n\n{ex.Message}",
                "L2Presence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateShortcutLabel()
    {
        _borderlessShortcutItem.Text =
            $"Toggle: {BorderlessSettingsForm.FormatShortcut(_borderlessSettings.ToggleShortcut)}";
    }

    private void ToggleBorderlessForTrayTarget()
    {
        if (_trayTargetWindow == IntPtr.Zero || !Win32.IsWindow(_trayTargetWindow))
        {
            MessageBox.Show(
                "Activate the target window, then open the tray menu again.",
                "L2Presence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _borderless.ToggleBorderless(_trayTargetWindow);
    }

    private void RunOnUiThread(Action action)
    {
        if (_uiThreadControl.IsDisposed)
            return;

        if (_uiThreadControl.InvokeRequired)
            _uiThreadControl.BeginInvoke(action);
        else
            action();
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

        _borderless.Dispose();
        _discord.Dispose();

        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        _trayIcon.Dispose();
        _uiThreadControl.Dispose();

        base.ExitThreadCore();
    }
}
