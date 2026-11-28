namespace Oekolariet.Domain.Aggregates.Avatars;

public sealed class Avatar
{
    // Saved games use this key even if the avatar's name or image changes.
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;

    // Inactive avatars can still appear on highscores from earlier rounds.
    public bool IsActive { get; private set; }

    private Avatar() { } // Used by EF Core.

    public static Avatar Create(string key, string name, string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        return new Avatar
        {
            Key = key.Trim(),
            Name = name.Trim(),
            ImageUrl = imageUrl.Trim(),
            IsActive = true
        };
    }
}
