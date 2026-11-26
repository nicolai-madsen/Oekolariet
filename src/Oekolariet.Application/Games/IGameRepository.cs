using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Application.Games;

public interface IGameRepository
{
    // The game is saved to the database when SaveChangesAsync is called.
    void Add(Game game);

    Task<Game?> GetByIdAsync(Guid gameId, CancellationToken ct); // Returns null if the game does not exist.
}
