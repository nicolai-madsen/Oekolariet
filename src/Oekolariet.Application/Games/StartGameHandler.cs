using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Application.Games;

public sealed class StartGameHandler(
    IExhibitionRepository exhibitions,
    IGameRepository games,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Guid> HandleAsync(
        Guid exhibitionId,
        string avatarKey,
        int guestNumber,
        TimeSpan duration,
        CancellationToken ct)
    {
        var exhibition = await exhibitions.GetByIdAsync(exhibitionId, ct);

        if (exhibition is null || exhibition.IsArchived)
        {
            throw new NotFoundException("Udstilling", exhibitionId);
        }

        if (exhibition.ActiveVersionId is not Guid versionId)
        {
            throw new ConflictException("Spillet kan først startes, når udstillingen er udgivet.");
        }

        // Keep this round tied to the version that was active at the start.
        var game = Game.Start(
            id: Guid.NewGuid(),
            versionId: versionId,
            guestNumber: guestNumber,
            avatarKey: avatarKey,
            startedAtUtc: clock.GetUtcNow().UtcDateTime,
            duration: duration);

        games.Add(game);
        await unitOfWork.SaveChangesAsync(ct);

        return game.Id;
    }
}
