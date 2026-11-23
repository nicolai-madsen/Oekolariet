using Microsoft.EntityFrameworkCore;
using Oekolariet.Domain.Aggregates.Drafts;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Aggregates.ExhibitionVersions;
using Oekolariet.Infrastructure.Converters;
using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Infrastructure;

public sealed class OekolarietDbContext(DbContextOptions<OekolarietDbContext> options) : DbContext(options)
{
    public DbSet<Exhibition> Exhibitions => Set<Exhibition>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<ExhibitionVersion> ExhibitionVersions => Set<ExhibitionVersion>();
    public DbSet<Draft> Drafts => Set<Draft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OekolarietDbContext).Assembly);
        // SQL Server assigns each game a guest number, even when games start at the same time.
        modelBuilder.HasSequence<int>("GuestNumbers")
            .StartsAt(1)
            .IncrementsBy(1);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQL doesnt remember DateTimeKind. Without it, "time" will be returned without a "Z" in the JSON, which we require for out format
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
}
