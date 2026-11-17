namespace Oekolariet.Shared.Exhibitions;

public sealed record ActiveVersionDto(
    Guid ExhibitionId,
    Guid VersionId,
    int VersionNumber,
    DateTime PublishedAt,
    ThemeDto Theme,
    IReadOnlyList<PageDto> Pages);

public sealed record ThemeDto(
    string PrimaryColor,
    string SecondaryColor,
    string FontFamily,
    Guid? LogoMediaId,
    Guid? BackgroundMediaId);

public sealed record PageDto(
    Guid Id,
    int Order,
    string Layout,   // store as a string, not the enum type, according to our api specification
    string Title,
    string Body,
    Guid? MediaId);
