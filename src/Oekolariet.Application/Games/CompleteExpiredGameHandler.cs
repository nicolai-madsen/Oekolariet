using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;

namespace Oekolariet.Application.Games;

public sealed class CompleteExpiredGameHandler(
    IGameRepository games,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task HandleAsync(Guid gameId, CancellationToken ct)
    {
        var game = await games.GetByIdAsync(gameId, ct);
        if (game is null)
        {
            throw new NotFoundException("Spil", gameId);
        }

        // The final answer may already have completed the round.
        if (game.CompletedAtUtc.HasValue)
        {
            return;
        }

        if (clock.GetUtcNow().UtcDateTime < game.EndsAt)
        {
            throw new ConflictException("Spillets tid er endnu ikke udløbet.");
        }

        // Use the deadline so a delayed request counts on the day the round ended.
        game.Complete(game.EndsAt);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
