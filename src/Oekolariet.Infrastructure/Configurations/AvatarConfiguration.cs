using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.Avatars;

namespace Oekolariet.Infrastructure.Configurations;

internal sealed class AvatarConfiguration : IEntityTypeConfiguration<Avatar>
{
    public void Configure(EntityTypeBuilder<Avatar> builder)
    {
        builder.ToTable("Avatars");

        // Saved games refer to the avatar by this stable key.
        builder.HasKey(avatar => avatar.Key);
        builder.Property(avatar => avatar.Key)
            .HasMaxLength(64)
            .ValueGeneratedNever();

        builder.Property(avatar => avatar.Name).IsRequired();
        builder.Property(avatar => avatar.ImageUrl).IsRequired();
    }
}
