namespace Oekolariet.Domain.Aggregates.Exhibitions;

public sealed class Exhibition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }
    public Guid? ActiveVersionId { get; private set; }

    private Exhibition() { }

    public static Exhibition Create(string name, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Navn er påkrævet.", nameof(name));
        }

        return new Exhibition
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAt = createdAtUtc
        };
    }
}
