using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Aggregates.ExhibitionVersions;

public sealed class ExhibitionVersion
{
    public Guid Id { get; private set; }
    public Guid ExhibitionId { get; private set; }
    public int VersionNumber { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public string PublishedBy { get; private set; } = string.Empty;
    public ExhibitionContent Content { get; private set; } = null!;

    private ExhibitionVersion() { } // Only for EF Core

    // internal: Only Exhibition.Publish can create new valid versions
    internal static ExhibitionVersion Create(
        Guid exhibitionId, int versionNumber, ExhibitionContent content, string publishedBy, DateTime publishedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (string.IsNullOrWhiteSpace(publishedBy))
        {
            throw new ArgumentException("PublishedBy er påkrævet.", nameof(publishedBy));
        }

        return new ExhibitionVersion
        {
            Id = Guid.NewGuid(),
            ExhibitionId = exhibitionId,
            VersionNumber = versionNumber,
            Content = content,
            PublishedBy = publishedBy,
            PublishedAt = publishedAtUtc,
        };
    }
}
