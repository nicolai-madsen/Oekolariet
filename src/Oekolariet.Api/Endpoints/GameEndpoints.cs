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

        return builder;
    }

    private static async Task<IResult> StartGameAsync(
        StartGameRequest request,
        StartGameHandler handler,
        CancellationToken ct)
    {
        // Use 30 seconds until the CMS provides the duration.
        var duration = TimeSpan.FromSeconds(30);

        var gameId = await handler.HandleAsync(
            request.ExhibitionId,
            request.AvatarKey,
            duration,
            ct);

        // TODO later: Return GameDto when the game is in ExhibitionContent
        return TypedResults.Created((string?)null, new { gameId });
    }
}
