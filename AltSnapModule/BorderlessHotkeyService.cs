using L2Presence.AltSnapModule.Actions;
using L2Presence.AltSnapModule.Input;
using L2Presence.AltSnapModule.Settings;

namespace L2Presence.AltSnapModule;

internal sealed class BorderlessHotkeyService : IDisposable
{
    private readonly BorderlessWindowStore _borderless = new();
    private readonly LowLevelInputHooks _hooks = new();
    private readonly BorderlessActionEngine _engine;
    private BorderlessSettings _settings;

    public BorderlessHotkeyService(BorderlessSettings settings)
    {
        _settings = settings.Clone();
        _engine = new BorderlessActionEngine(_settings, _borderless);
        _engine.Faulted += HandleFault;
    }

    public bool Enabled { get; private set; }
    public string? LastError { get; private set; }
    public string Status => Enabled ? "Borderless hotkey: on" : "Borderless hotkey: off";

    public event EventHandler? StatusChanged;

    public void Start()
    {
        if (Enabled)
            return;

        LastError = null;
        _engine.ResetInputState();
        try
        {
            _hooks.Keyboard += HandleKeyboard;
            _hooks.MouseButton += HandleMouseButton;
            _hooks.Install();
            Enabled = true;
        }
        catch
        {
            DetachHooks();
            throw;
        }

        OnStatusChanged();
    }

    public void Stop()
    {
        if (!Enabled)
            return;

        DetachHooks();
        Enabled = false;
        OnStatusChanged();
    }

    public void ApplySettings(BorderlessSettings settings)
    {
        _settings = settings.Clone();
        _engine.ApplySettings(_settings);
    }

    public bool ToggleBorderless(IntPtr hWnd) => _borderless.Toggle(hWnd);

    public void RestoreAllBorders() => _borderless.RestoreAll();

    public void Dispose()
    {
        Stop();
        _engine.Faulted -= HandleFault;
        _borderless.RestoreAll();
    }

    private bool HandleKeyboard(KeyboardHookEvent hookEvent)
        => _engine.HandleKeyboard(hookEvent);

    private bool HandleMouseButton(MouseButtonHookEvent hookEvent)
        => _engine.HandleMouseButton(hookEvent);

    private void HandleFault(Exception exception)
    {
        LastError = exception.Message;
        OnStatusChanged();
    }

    private void DetachHooks()
    {
        _hooks.Keyboard -= HandleKeyboard;
        _hooks.MouseButton -= HandleMouseButton;
        _hooks.Dispose();
    }

    private void OnStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);
}
