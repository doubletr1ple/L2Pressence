using System.Text.Json;

namespace L2Presence.AltSnapModule.Settings;

internal sealed class BorderlessSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;

    public BorderlessSettingsStore()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "L2Presence");
        _path = Path.Combine(directory, "borderless-settings.json");
    }

    public BorderlessSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
                return new BorderlessSettings();

            var settings = JsonSerializer.Deserialize<BorderlessSettings>(
                File.ReadAllText(_path),
                JsonOptions) ?? new BorderlessSettings();

            if (!IsValid(settings.ToggleShortcut))
                settings.ToggleShortcut = new BorderlessShortcut();

            return settings;
        }
        catch
        {
            return new BorderlessSettings();
        }
    }

    public void Save(BorderlessSettings settings)
    {
        var directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);

        var temporaryPath = _path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporaryPath, _path, true);
    }

    private static bool IsValid(BorderlessShortcut? shortcut)
        => shortcut is not null &&
           shortcut.Modifiers != HotkeyModifiers.None &&
           (shortcut.Modifiers & ~((HotkeyModifiers)15)) == 0 &&
           Enum.IsDefined(shortcut.Button);
}
