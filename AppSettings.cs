namespace L2Presence;

internal sealed class AppSettings
{
    public string DiscordApplicationId { get; } = "1538990029031084183";
    public string ProcessName { get; } = "l2";
    public string ServerName { get; } = "ElmoreLab Erica";
    public int PollIntervalSeconds { get; } = 2;

    public string LargeImageKey { get; } = "l2_epilogue";
    public string LargeImageText { get; } = "Lineage II • ElmoreLab Erica";

    // Leave both empty if the Lineage 2 window title is exactly the character name.
    // Example title: "Lineage II - MyCharacter"
    // CharacterNamePrefixToRemove = "Lineage II - "
    public string CharacterNamePrefixToRemove { get; } = "";
    public string CharacterNameSuffixToRemove { get; } = "";

    public void Validate()
    {
        if (!ulong.TryParse(DiscordApplicationId, out _))
            throw new InvalidOperationException("DiscordApplicationId must contain digits only.");

        if (string.IsNullOrWhiteSpace(ProcessName))
            throw new InvalidOperationException("ProcessName cannot be empty.");

        if (PollIntervalSeconds < 1)
            throw new InvalidOperationException("PollIntervalSeconds must be at least 1.");
    }
}

