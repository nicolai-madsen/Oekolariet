using Microsoft.AspNetCore.Http.HttpResults;
using Oekolariet.Application.Auth;
using Oekolariet.Shared.Auth;

// The login endpoint: POST /api/v1/auth/login (openapi.yaml: login)
internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/auth").WithTags("auth");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .AllowAnonymous() // you must be able to log in without being logged in
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return builder;
    }

    // 200 + token if the email and password are correct, 401 if not
    private static async Task<Results<Ok<LoginResponseDto>, UnauthorizedHttpResult>> LoginAsync(
        LoginRequestDto request,
        LoginHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(request, ct);

        if (response is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(response);
    }
}
