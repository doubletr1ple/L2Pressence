using L2Presence.AltSnapModule.Input;
using L2Presence.AltSnapModule.Interop;
using L2Presence.AltSnapModule.Settings;

namespace L2Presence.AltSnapModule.Actions;

internal sealed class BorderlessActionEngine
{
    private readonly ModifierState _modifiers = new();
    private readonly BorderlessWindowStore _borderless;
    private BorderlessSettings _settings;
    private MouseButtonKind? _suppressedButton;

    public BorderlessActionEngine(BorderlessSettings settings, BorderlessWindowStore borderless)
    {
        _settings = settings;
        _borderless = borderless;
        _modifiers.ResetFromSystem();
    }

    public event Action<Exception>? Faulted;

    public void ApplySettings(BorderlessSettings settings) => _settings = settings;

    public void ResetInputState()
    {
        _suppressedButton = null;
        _modifiers.ResetFromSystem();
    }

    public bool HandleKeyboard(KeyboardHookEvent hookEvent)
    {
        _modifiers.Update(hookEvent.VirtualKey, hookEvent.IsDown);
        return false;
    }

    public bool HandleMouseButton(MouseButtonHookEvent hookEvent)
    {
        if (!hookEvent.IsDown && hookEvent.Button == _suppressedButton)
        {
            _suppressedButton = null;
            return true;
        }

        if (!hookEvent.IsDown || !MatchesShortcut(hookEvent.Button))
            return false;

        var hWnd = WindowFinder.GetRootWindowFromPoint(hookEvent.ScreenPoint);
        if (hWnd == IntPtr.Zero)
            return false;

        try
        {
            _borderless.Toggle(hWnd);
            _suppressedButton = hookEvent.Button;
            return true;
        }
        catch (Exception ex)
        {
            Faulted?.Invoke(ex);
            return false;
        }
    }

    private bool MatchesShortcut(MouseButtonKind button)
        => _settings.ToggleShortcut.Button == button &&
           _settings.ToggleShortcut.Modifiers == _modifiers.Snapshot();
}
