using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Oekolariet.Application.Auth;
using Oekolariet.Domain.Aggregates.Editors;
using Oekolariet.Shared.Auth;

namespace Oekolariet.Api.Auth;

// Creates the JWT (login token) that an editor gets after a correct login.
// configuration = reads appsettings.json and user-secrets. clock = gives the current time.
// Both are given to the class by DI (registered in Program.cs).
internal sealed class JwtAccessTokenGenerator(IConfiguration configuration, TimeProvider clock) : IAccessTokenGenerator
{
    public LoginResponseDto Create(Editor editor)
    {
        // 1. What the token says about the editor (claims). Anyone can read these, but nobody can change them.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, editor.Id.ToString()), // "sub" (subject) = who the token is about: the editor's id
            new Claim(JwtRegisteredClaimNames.Email, editor.Email),       // the email, used later to see who changed a draft
            new Claim(ClaimTypes.Role, Roles.Editor),                     // the role, used later to allow CMS endpoints
        };

        // 2. The signature. Only someone with our secret key can make it, so nobody can make a fake token.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)); // the secret key (from user-secrets) as bytes
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);           // sign with this key, using HS256

        // 3. When the token stops working: now + LifetimeHours (8 hours, from appsettings.json)
        var lifetimeHours = configuration.GetValue<int>("Jwt:LifetimeHours");
        var expiresAt = clock.GetUtcNow().UtcDateTime.AddHours(lifetimeHours);

        // 4. Build the token. Issuer and Audience must be the same as the ones Program.cs checks.
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],     // who made the token: our API
            audience: configuration["Jwt:Audience"], // who the token is for: the CMS
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        // 5. Turn the token into one long text string: header.payload.signature (like "eyJhbGci...")
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // 6. Send the token and its expiry back. The CMS puts the token in "Authorization: Bearer ..." from now on.
        return new LoginResponseDto(accessToken, expiresAt);
    }
}
