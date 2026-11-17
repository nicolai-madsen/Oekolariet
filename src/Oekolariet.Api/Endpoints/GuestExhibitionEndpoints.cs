using Microsoft.AspNetCore.Http.HttpResults;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Api.Endpoints;

internal static class GuestExhibitionEndpoints
{
    public static IEndpointRouteBuilder MapGuestExhibitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/exhibitions").WithTags("guest");

        group.MapGet("/{exhibitionId:guid}/active-version", GetActiveVersionAsync)
            .WithName("GetActiveVersion")
            .AllowAnonymous();

        return app;
    }

    private static async Task<Results<Ok<ActiveVersionDto>, NotFound>> GetActiveVersionAsync(
        Guid exhibitionId, GetActiveVersionHandler handler, CancellationToken ct)
    {
        var dto = await handler.HandleAsync(exhibitionId, ct);

        if (dto is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(dto);
    }
}
