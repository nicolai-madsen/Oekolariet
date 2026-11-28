using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Avatars;
using Oekolariet.Shared.Games;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class AvatarRepository(OekolarietDbContext db) : IAvatarRepository
{
    public Task<List<AvatarDto>> GetByKeysAsync(
        IReadOnlyCollection<string> keys,
        CancellationToken ct)
    {
        // Past scores still show their avatar after it is deactivated in the CMS.
        return db.Avatars
            .Where(avatar => keys.Contains(avatar.Key))
            .Select(avatar => new AvatarDto(avatar.Key, avatar.Name, avatar.ImageUrl))
            .ToListAsync(ct);
    }
}
