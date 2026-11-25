using Microsoft.AspNetCore.Http.HttpResults;
using Oekolariet.Application.Drafts;
using Oekolariet.Shared.Drafts;
using Okelariet.Application.Drafts;

internal static class DraftEndpoints
{
    public static IEndpointRouteBuilder MapDraftEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/exhibitions/{exhibitionId:guid}/draft").WithTags("draft");

        group.MapPost("/", CreateDraftAsync)
            .WithName("CreateDraft")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", GetDraftAsync)
            .WithName("GetDraft")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Created<DraftDto>> CreateDraftAsync(Guid exhibitionId, CreateDraftHandler handler, CancellationToken ct)
    {
        const string UserEmail = "dev@oekolariet.com"; // TODO: Get this from JWT, when login is in place

        var dto = await handler.HandleAsync(exhibitionId, UserEmail, ct);

        return TypedResults.Created($"/api/v1/exhibitions/{exhibitionId}/draft", dto);
    }

    private static async Task<Ok<DraftDto>> GetDraftAsync(Guid exhibitionId, GetDraftHandler handler, CancellationToken ct)
    {
        var dto = await handler.HandleAsync(exhibitionId, ct);
        return TypedResults.Ok(dto);
    }
}
