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
}
