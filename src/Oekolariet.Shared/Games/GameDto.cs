using System.Text.Json.Serialization;

namespace Oekolariet.Shared.Games;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SortingGameDto), "Sorting")]
public abstract record GameDto(
    Guid GameId,
    Guid VersionId,
    DateTime EndsAtUtc);

public sealed record SortingGameDto(
    Guid GameId,
    Guid VersionId,
    DateTime EndsAtUtc,
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<GuestItemDto> Items)
    : GameDto(GameId, VersionId, EndsAtUtc);
