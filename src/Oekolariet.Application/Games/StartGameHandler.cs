using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Common.Mapping;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.Games;
using Oekolariet.Domain.Content;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Games;

// Handler for starting a new game for a given exhibition.
public sealed class StartGameHandler(
    IExhibitionRepository exhibitions,
    IExhibitionVersionRepository versions,
    IGameRepository games,
    IGuestNumberGenerator guestNumbers,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<GameDto> HandleAsync(
        Guid exhibitionId,
        string avatarKey,
        CancellationToken ct)
    {
        
        var exhibition = await exhibitions.GetByIdAsync(exhibitionId, ct);

        // Check if the exhibition exists and is not archived (archived counts as deleted)
        if (exhibition is null || exhibition.IsArchived)
        {
            throw new NotFoundException("Udstilling", exhibitionId);
        }

        // Check if the exhibition has an active version (published)
        if (exhibition.ActiveVersionId is not Guid versionId)
        {
            throw new ConflictException("Spillet kan først startes, når udstillingen er udgivet.");
        }

        var exhibitionVersion = await versions.GetByIdAsync(versionId, ct);
        // Check if the active version exists in the database
        if (exhibitionVersion is null)
        {
            throw new InvalidOperationException($"Aktiv version {versionId} mangler i databasen.");
        }

        var content = exhibitionVersion.Content.Game;
        var guestNumber = await guestNumbers.NextAsync(ct);

        // Use the same version for the entire round, even if a new one is published.
        // The game is a snapshot of the version at the time it was started.
        var game = Game.Start(
            id: Guid.NewGuid(),
            versionId: versionId,
            guestNumber: guestNumber,
            avatarKey: avatarKey,
            startedAtUtc: clock.GetUtcNow().UtcDateTime,
            duration: TimeSpan.FromSeconds(content.DurationSeconds));

        games.Add(game);
        await unitOfWork.SaveChangesAsync(ct);

        return ToDto(game, content);
    }

    // Returns a GameDto (the base type), so the JSON gets the "type": "Sorting"
    // This Dto is for the guest, so it does NOT include CorrectCategoryId or Explanation.
    private static GameDto ToDto(Game game, SortingGame content) =>
    new SortingGameDto(
        game.Id,
        game.VersionId,
        game.EndsAt,
        content.Categories
            .Select(category => category.ToDto())
            .ToList(),
        // ADR-0003: Only expose item details the guest may see before answering.
        content.Items
            .Select(item => new GuestItemDto(item.Id, item.Name, item.MediaId))
            .ToList());
}
