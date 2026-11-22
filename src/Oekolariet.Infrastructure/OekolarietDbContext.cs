using Microsoft.EntityFrameworkCore;
using Oekolariet.Domain.Aggregates.Drafts;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Aggregates.ExhibitionVersions;
using Oekolariet.Infrastructure.Converters;

namespace Oekolariet.Infrastructure;

public sealed class OekolarietDbContext(DbContextOptions<OekolarietDbContext> options) : DbContext(options)
{
    public DbSet<Exhibition> Exhibitions => Set<Exhibition>();
    public DbSet<ExhibitionVersion> ExhibitionVersions => Set<ExhibitionVersion>();
    public DbSet<Draft> Drafts => Set<Draft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OekolarietDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQL doesnt remember DateTimeKind. Without it, "time" will be returned without a "Z" in the JSON, which we require for out format
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
}
