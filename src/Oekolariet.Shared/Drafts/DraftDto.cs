using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Shared.Drafts;

public sealed record DraftDto(
    Guid ExhibitionId,
    Guid? BasedOnVersionId,
    DateTime UpdatedAt,
    string Updatedby,
    ThemeDto Theme,
    IReadOnlyList<PageDto> Pages);
