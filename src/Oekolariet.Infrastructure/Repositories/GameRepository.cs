using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Games;
using Oekolariet.Domain.Aggregates.Games;
using Oekolariet.Shared.Games;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class GameRepository(OekolarietDbContext db) : IGameRepository
{
    public void Add(Game game)
    {
        // StartGameHandler saves the game through IUnitOfWork.
        db.Games.Add(game);
    }

    public Task<Game?> GetByIdAsync(Guid gameId, CancellationToken ct)
    {
        // Track the game so the unit of work can save its updated score.
        return db.Games.AsTracking().SingleOrDefaultAsync(game => game.Id == gameId, ct);
    }

    public async Task<List<HighscoreEntryDto>> GetHighscoresAsync(
    Guid versionId,
    DateTime fromUtc,
    DateTime toUtc,
    CancellationToken ct)
    {
        return await db.Games
            .Where(game => game.VersionId == versionId
                && game.CompletedAtUtc.HasValue
                && game.CompletedAtUtc.Value >= fromUtc
                && game.CompletedAtUtc.Value < toUtc)
            .OrderByDescending(game => game.Score)
            .ThenByDescending(game => game.CompletedAtUtc)
            .ThenBy(game => game.GuestNumber)
            .Select(game => new HighscoreEntryDto(
                game.AvatarKey,
                game.GuestNumber,
                game.Score))
            .ToListAsync(ct);
    }
}
