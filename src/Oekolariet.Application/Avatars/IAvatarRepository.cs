using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Avatars;

public interface IAvatarRepository
{
    Task<List<AvatarDto>> GetByKeysAsync(
        IReadOnlyCollection<string> keys,
        CancellationToken ct);
}
