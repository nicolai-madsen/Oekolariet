using System.Text.Json.Serialization;
using Oekolariet.Shared.Games;

namespace Oekolariet.Shared.Drafts;

// Polymorphic: EditorGameDto is the family name, and each game type is a family member.
// Right now, there's only the Sorting Game type, but the system is open for new types (but the api spec is built with oneOf + discriminator),
// which can be added without changing DraftDto.

// Json adds type fields to say which family member is which and create matching subclasses for them
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")] // the field name in JSON
[JsonDerivedType(typeof(SortingEditorGameDto), "Sorting")] // "Sorting" in JSON <--> this C# type
public abstract record EditorGameDto(string Title, string Intro);

public sealed record SortingEditorGameDto(
    string Title,
    string Intro,
    int DurationSeconds,
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<EditorItemDto> Items) // EditorItemDto has CorrectCategoryId (CMS only, requires login)
        : EditorGameDto(Title, Intro); // Passes the Title and Intro on to the base record
