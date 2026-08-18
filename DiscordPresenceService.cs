using DiscordRPC;

namespace L2Presence;

internal sealed class DiscordPresenceService : IDisposable
{
    private readonly DiscordRpcClient _client;
    private readonly string _serverName;
    private readonly string _largeImageKey;
    private readonly string _largeImageText;
    private string? _currentSignature;
    private DateTime? _sessionStartUtc;
    private bool _presenceIsSet;

    public DiscordPresenceService(AppSettings settings)
    {
        _serverName = settings.ServerName;
        _largeImageKey = settings.LargeImageKey;
        _largeImageText = settings.LargeImageText;
        _client = new DiscordRpcClient(settings.DiscordApplicationId);
    }

    public void Initialize()
    {
        _client.Initialize();
    }

    public void UpdateCharacters(IReadOnlyList<L2WindowInfo> windows)
    {
        if (windows.Count == 0)
        {
            Clear();
            return;
        }

        var characters = windows
            .Select(x => x.CharacterName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (characters.Count == 0)
        {
            Clear();
            return;
        }

        var signature = string.Join("\u001F", characters);
        if (string.Equals(_currentSignature, signature, StringComparison.Ordinal))
            return;

        _currentSignature = signature;
        _sessionStartUtc ??= DateTime.UtcNow;

        var characterText = characters.Count == 1
            ? $"Character: {characters[0]}"
            : $"Characters: {string.Join(", ", characters)}";

        if (characterText.Length > 128)
            characterText = characterText[..125] + "...";

        var state = characters.Count == 1
            ? _serverName
            : $"{characters.Count} clients • {_serverName}";

        if (state.Length > 128)
            state = state[..125] + "...";

        var presence = new RichPresence
        {
            Details = characterText,
            State = state,
            Timestamps = new Timestamps
            {
                Start = _sessionStartUtc.Value
            }
        };

        if (!string.IsNullOrWhiteSpace(_largeImageKey))
        {
            presence.Assets = new Assets
            {
                LargeImageKey = _largeImageKey,
                LargeImageText = string.IsNullOrWhiteSpace(_largeImageText)
                    ? null
                    : _largeImageText
            };
        }

        _client.SetPresence(presence);
        _presenceIsSet = true;
    }

    public void Clear()
    {
        if (!_presenceIsSet)
            return;

        _client.ClearPresence();
        _presenceIsSet = false;
        _currentSignature = null;
        _sessionStartUtc = null;
    }

    public void Dispose()
    {
        try
        {
            Clear();
        }
        finally
        {
            _client.Dispose();
        }
    }
}
