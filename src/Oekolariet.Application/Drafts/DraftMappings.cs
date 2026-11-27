using Oekolariet.Application.Common.Mapping;
using Oekolariet.Domain.Aggregates.Drafts;
using Oekolariet.Domain.Content;
using Oekolariet.Shared.Drafts;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Drafts;

// Converts a Draft object to a DraftDto object.

// The mapping is done in a separate static class to keep the CreateDraftHandler class focused on its main responsibility of handling the creation of drafts.
// This separation of concerns (SRP, WOHOO) makes the code easier to read and maintain.
internal static class DraftMappings
{
    public static DraftDto ToDto(this Draft draft) =>
        new (
            draft.ExhibitionId,
            draft.BasedOnVersionId,
            draft.UpdatedAt,
            draft.UpdatedBy,
            draft.Content.Theme.ToDto(), // From ContentMappings
            draft.Content.Pages.ToDtos(), // FRom ContentMappings
            draft.Content.Tiles.ToDtos(), // From ContentMappings
            draft.Content.Game.ToEditorDto());

    // Editor version WITH CorrectCategoryId + Explanation, used for editing the game in the draft.
    // Deliberately named ToEditorDto to try and avoid confusion with the regular ToDto mapping for games, which is for the guest.
    private static SortingEditorGameDto ToEditorDto(this SortingGame game) =>
        new(
            game.Title,
            game.Intro,
            game.DurationSeconds,
            game.Categories.Select(c => c.ToDto()).ToList(),
            game.Items.Select(i => new EditorItemDto(i.Id, i.Name, i.MediaId, i.CorrectCategoryId, i.Explanation)).ToList());
}
