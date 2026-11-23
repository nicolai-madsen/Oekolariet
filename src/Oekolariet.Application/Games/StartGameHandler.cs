using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Application.Games;

public sealed class StartGameHandler(
    IExhibitionRepository exhibitions,
    IGameRepository games,
    IGuestNumberGenerator guestNumbers,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Guid> HandleAsync(
        Guid exhibitionId,
        string avatarKey,
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

        var guestNumber = await guestNumbers.NextAsync(ct);

        // Use the same version for the entire round, even if a new one is published.
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
