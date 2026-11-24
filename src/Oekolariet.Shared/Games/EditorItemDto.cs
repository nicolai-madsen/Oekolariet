namespace Oekolariet.Shared.Games;

public sealed record EditorItemDto(
    Guid Id,
    string Name,
    Guid MediaId,
    Guid CorrectCategoryId,
    string Explanation);
