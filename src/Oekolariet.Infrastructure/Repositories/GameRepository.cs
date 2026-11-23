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
}
