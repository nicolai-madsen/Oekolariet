using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.Drafts;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Infrastructure.Converters;

namespace Oekolariet.Infrastructure.Configurations;

internal sealed class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder)
    {
        builder.ToTable("Drafts");

        // ExhibitionId is the pk, so the db litrally can't hold two drafts per exhibition
        builder.HasKey(d => d.ExhibitionId);
        builder.Property(d => d.ExhibitionId).ValueGeneratedNever();

        builder.HasOne<Exhibition>()
            .WithOne()
            .HasForeignKey<Draft>(d => d.ExhibitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.UpdatedBy).HasMaxLength(256).IsRequired();

        builder.Property(d => d.Content)
            .HasConversion(new ExhibitionContentJsonConverter())
            .HasColumnName("ContentJson")
            .IsRequired();
    }
}
