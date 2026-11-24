using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.Drafts;
using Oekolariet.Domain.Content;
using Oekolariet.Shared.Drafts;
using Oekolariet.Shared.Exhibitions;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Drafts;

public sealed class CreateDraftHandler(
    IExhibitionRepository exhibitions,
    IExhibitionVersionRepository versions,
    IDraftRepository drafts,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<DraftDto> HandleAsync(Guid exhibitionId, string userEmail, CancellationToken ct)
    {
        // Find the exhibition (archived counts as deleted)
        var exhibition = await exhibitions.GetByIdAsync(exhibitionId, ct);
        if (exhibition is null || exhibition.IsArchived)
        {
            throw new NotFoundException("Udstilling", exhibitionId);
        }

        // Enforce "max one draft per exhibition" rule
        if (await drafts.GetByExhibitionIdAsync(exhibitionId, ct) is not null)
        {
            throw new ConflictException("Der findes allerede en kladde til denne udstilling.");
        }

        // Make a copy of the active version, or empty
        var content = ExhibitionContent.Empty;
        Guid? basedOnVersionId = null;

        if (exhibition.ActiveVersionId is Guid activeVersionId)
        {
            var activeVersion = await versions.GetByIdAsync(activeVersionId, ct)
                ?? throw new InvalidOperationException($"Aktiv version {activeVersionId} mangler i databasen.");

            content = activeVersion.Content;
            basedOnVersionId = activeVersion.Id;
        }

        // Create and save the draft
        var draft = Draft.Create(exhibitionId, basedOnVersionId, content, userEmail, clock.GetUtcNow().UtcDateTime);
        drafts.Add(draft);
        await unitOfWork.SaveChangesAsync(ct);

        return ToDto(draft);
    }

    private static DraftDto ToDto(Draft draft)
    {
        var theme = draft.Content.Theme;
        var game = draft.Content.Game;

        return
        new DraftDto(
            draft.ExhibitionId,
            draft.BasedOnVersionId,
            draft.UpdatedAt,
            draft.UpdatedBy,
            new ThemeDto(theme.PrimaryColor, theme.SecondaryColor, theme.FontFamily, theme.LogoMediaId, theme.BackgroundMediaId),
            draft.Content.Pages
                .OrderBy(p => p.Order)
                .Select(p => new PageDto(p.Id, p.Order, p.Layout.ToString(), p.Title, p.Body, p.MediaId))
                .ToList(),

        new SortingEditorGameDto(
            game.Title,
            game.Intro,
            game.DurationSeconds,
            game.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.MediaId)).ToList(),
            game.Items.Select(i => new EditorItemDto(i.Id, i.Name, i.MediaId, i.CorrectCategoryId, i.Explanation)).ToList()));

    }
}
