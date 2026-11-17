using Oekolariet.Domain.Aggregates.ExhibitionVersions;
using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Aggregates.Exhibitions;

public sealed class Exhibition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }
    public Guid? ActiveVersionId { get; private set; }
    public int LatestVersionNumber { get; private set; }

    private Exhibition() { } // Kun til EF Core

    public static Exhibition Create(Guid id, string name, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id må ikke være tomt.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Navn er påkrævet.", nameof(name));
        }

        return new Exhibition
        {
            Id = id,
            Name = name.Trim(),
            CreatedAt = createdAtUtc,
        };
    }

    public ExhibitionVersion Publish(ExhibitionContent content, string publishedBy, DateTime publishedAtUtc)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("En arkiveret udstilling kan ikke udgives.");
        }

        LatestVersionNumber++;
        var version = ExhibitionVersion.Create(Id, LatestVersionNumber, content, publishedBy, publishedAtUtc);
        ActiveVersionId = version.Id;
        return version;
    }
}
