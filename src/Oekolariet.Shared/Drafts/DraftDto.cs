using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Shared.Drafts;

public sealed record DraftDto(
    Guid ExhibitionId,
    Guid? BasedOnVersionId,
    DateTime UpdatedAt,
    string UpdatedBy,
    ThemeDto Theme,
    IReadOnlyList<PageDto> Pages,
    IReadOnlyList<TileDto> Tiles,
    EditorGameDto Game); // Just the basis type, so the "type" gets added in Json automatically
