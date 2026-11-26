using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.ExhibitionVersions;
using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Infrastructure.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.AvatarKey).IsRequired();

        // Multiple games can use the same version. Keep it while any game references it.
        builder.HasOne<ExhibitionVersion>()
            .WithMany()
            .HasForeignKey(g => g.VersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(game => game.Answers, answers =>
        {
            answers.ToTable("GameAnswers");
            answers.WithOwner().HasForeignKey(answer => answer.GameId);

            // An item can be answered once per game and used again in another game.
            answers.HasKey(answer => new { answer.GameId, answer.ItemId });
            answers.Property(answer => answer.ItemId).ValueGeneratedNever();
        });

        builder.Navigation(game => game.Answers) // Configure the private field for EF Core to use when loading a saved game.
            .HasField("_answers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
