namespace Oekolariet.Shared.Auth;

// What the API sends back to the caller (the CMS) after a successful login (openapi.yaml: LoginResponse)
public sealed record LoginResponseDto(string AccessToken, DateTime ExpiresAt);
