namespace Oekolariet.Shared.Games;

public sealed record AnswerResultDto(
    bool IsCorrect,
    Guid CorrectCategoryId,
    string? Explanation,
    int AnsweredCount,
    int TotalCount,
    bool IsGameCompleted);
