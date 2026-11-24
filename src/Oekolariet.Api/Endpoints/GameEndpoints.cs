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
}
