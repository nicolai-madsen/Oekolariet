namespace Oekolariet.Shared.Games;

public sealed record GuestItemDto(
    Guid Id,
    string Name,
    Guid MediaId);
