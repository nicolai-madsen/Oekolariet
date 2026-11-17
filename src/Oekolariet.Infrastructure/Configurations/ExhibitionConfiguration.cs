using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.Exhibitions;

namespace Oekolariet.Infrastructure.Configurations;

internal sealed class ExhibitionConfiguration : IEntityTypeConfiguration<Exhibition>
{
    public void Configure(EntityTypeBuilder<Exhibition> builder)
    {
        builder.ToTable("Exhibitions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        // ActiveVersionId is NOT an FK, cuz that'd create a circular dependency,
        // which EF Core wont accept into one SaveChanges() transaction.
    }
}
