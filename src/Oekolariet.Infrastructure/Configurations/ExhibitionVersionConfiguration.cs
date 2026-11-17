using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Aggregates.ExhibitionVersions;
using Oekolariet.Infrastructure.Converters;

namespace Oekolariet.Infrastructure.Configurations;

internal sealed class ExhibitionVersionConfiguration : IEntityTypeConfiguration<ExhibitionVersion>
{
    public void Configure(EntityTypeBuilder<ExhibitionVersion> builder)
    {
        builder.ToTable("ExhibitionVersions");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.Property(v => v.PublishedBy).HasMaxLength(256).IsRequired();

        // FK in db, but no navigation in the domain (only referenced via Id)
        builder.HasOne<Exhibition>()
            .WithMany()
            .HasForeignKey(v => v.ExhibitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Just to be sure version numbers are unique per exhibition
        builder.HasIndex(v => new { v.ExhibitionId, v.VersionNumber }).IsUnique();

        builder.Property(v => v.Content)
            .HasConversion(new ExhibitionContentJsonConverter())
            .HasColumnName("ContentJson")
            .IsRequired();
    }
}
