using System.Diagnostics;

namespace L2Presence;

internal sealed record L2WindowInfo(int ProcessId, string WindowTitle, string CharacterName);

internal sealed class L2WindowDetector
{
    private readonly AppSettings _settings;

    public L2WindowDetector(AppSettings settings)
    {
        _settings = settings;
    }

    public IReadOnlyList<L2WindowInfo> DetectAll()
    {
        var result = new List<L2WindowInfo>();

        foreach (var process in Process.GetProcessesByName(_settings.ProcessName))
        {
            using (process)
            {
                try
                {
                    var title = process.MainWindowTitle?.Trim();
                    if (string.IsNullOrWhiteSpace(title))
                        continue;

                    var characterName = ParseCharacterName(title);
                    if (string.IsNullOrWhiteSpace(characterName))
                        continue;

                    result.Add(new L2WindowInfo(process.Id, title, characterName));
                }
                catch (InvalidOperationException)
                {
                    // Process exited while being inspected.
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // Process became inaccessible between enumeration and inspection.
                }
            }
        }

        return result
            .OrderBy(x => x.ProcessId)
            .ToList();
    }

    private string ParseCharacterName(string title)
    {
        var name = title.Trim();

        if (!string.IsNullOrEmpty(_settings.CharacterNamePrefixToRemove) &&
            name.StartsWith(_settings.CharacterNamePrefixToRemove, StringComparison.OrdinalIgnoreCase))
        {
            name = name[_settings.CharacterNamePrefixToRemove.Length..];
        }

        if (!string.IsNullOrEmpty(_settings.CharacterNameSuffixToRemove) &&
            name.EndsWith(_settings.CharacterNameSuffixToRemove, StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^_settings.CharacterNameSuffixToRemove.Length];
        }

        return name.Trim();
    }
}
