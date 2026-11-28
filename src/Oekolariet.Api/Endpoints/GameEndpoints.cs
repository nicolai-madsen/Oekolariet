using Microsoft.AspNetCore.Http.HttpResults;
using Oekolariet.Application.Games;
using Oekolariet.Shared.Games;

internal static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/games").WithTags("games");

        group.MapPost("/", StartGameAsync)
            .WithName("StartGame")
            .AllowAnonymous()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{gameId:guid}/answers", SubmitAnswerAsync)
            .WithName("AnswerItem")
            .AllowAnonymous()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{gameId:guid}/highscores", GetHighscoresAsync)
            .WithName("GetGameHighscores")
            .AllowAnonymous()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Created<GameDto>> StartGameAsync(
        StartGameRequest request,
        StartGameHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(request.ExhibitionId, request.AvatarKey, ct);
        return TypedResults.Created((string?)null, dto);
    }

    private static async Task<Ok<AnswerResultDto>> SubmitAnswerAsync(
    Guid gameId,
    AnswerRequest request,
    SubmitAnswerHandler handler,
    CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            gameId,
            request.ItemId,
            request.CategoryId,
            ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<List<HighscoreEntryDto>>, ProblemHttpResult>>
    GetHighscoresAsync(
        Guid gameId,
        GetHighscoresHandler handler,
        CancellationToken ct,
        string? period = null)
    {
        HighscorePeriod? selectedPeriod = period?.ToLowerInvariant() switch
        {
            null or "day" => HighscorePeriod.Day,
            "month" => HighscorePeriod.Month,
            _ => null
        };

        if (selectedPeriod is null)
        {
            return TypedResults.Problem(
                detail: "Perioden skal være 'day' eller 'month'.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var entries = await handler.HandleAsync(gameId, selectedPeriod.Value, ct);
        return TypedResults.Ok(entries);
    }
}
