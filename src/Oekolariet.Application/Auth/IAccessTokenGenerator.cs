using Oekolariet.Domain.Aggregates.Editors;
using Oekolariet.Shared.Auth;

namespace Oekolariet.Application.Auth;

// Creates the login token (JWT) for an editor. Application only knows this promise.
public interface IAccessTokenGenerator
{
    LoginResponseDto Create(Editor editor);
}
