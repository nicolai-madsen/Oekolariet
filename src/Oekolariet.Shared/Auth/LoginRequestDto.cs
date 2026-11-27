namespace Oekolariet.Shared.Auth;

// What the CMS sends to log in (openapi.yaml: LoginRequest)
public sealed record LoginRequestDto(string Email, string Password);
