namespace Oekolariet.Shared.Games;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    Guid? MediaId);
