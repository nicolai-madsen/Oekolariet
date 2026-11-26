using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Games;
using Oekolariet.Domain.Aggregates.Games;

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
}
