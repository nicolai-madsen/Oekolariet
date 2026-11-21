using Oekolariet.Domain.Content;

public sealed class Draft
{
    public Guid ExhibitionId { get; private set; } // also PK, to limit to max 1 draft per exhibition
    public Guid? BasedOnVersionId { get; private set; }
    public ExhibitionContent Content { get; private set; } = null!;
    public DateTime UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; } = string.Empty;

    public Draft() { }

    // Static, because method creates a new object
    public static Draft Create(Guid exhibitionId, Guid? basedOnVersionId, ExhibitionContent content, string updatedBy, DateTime updatedAtUtc)
    {
        if (exhibitionId == Guid.Empty)
        {
            throw new ArgumentException("ExhibitionId må ikke være tomt.", nameof(exhibitionId));
        }

        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            throw new ArgumentException("UpdateBy er påkrævet.", nameof(updatedBy));
        }

        return new Draft
        {
            ExhibitionId = exhibitionId,
            BasedOnVersionId = basedOnVersionId,
            Content = content,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAtUtc
        };
    }

    // non static, because method replaces specific instance of an object
    public void Replace(ExhibitionContent content, string updatedBy, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            throw new ArgumentException("UpdatedBy er påkrævet.", nameof(updatedBy));
        }

        Content = content;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAtUtc;
    }
}
