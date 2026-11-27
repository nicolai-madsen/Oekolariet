namespace Oekolariet.Shared.Games;

public sealed record HighscoreEntryDto(
    string AvatarKey,
    int GuestNumber,
    int Score);
