using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Application.Common;
using Oekolariet.Application.Drafts;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Application.Games;
using Oekolariet.Infrastructure.Persistence;
using Oekolariet.Infrastructure.Repositories;
using Oekolariet.Infrastructure.Seeding;


namespace Oekolariet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, bool seedDevelopmentData)
    {
        services.AddDbContext<OekolarietDbContext>(options =>
        {
            options.UseSqlServer(connectionString);

            if (seedDevelopmentData)
            {
                options.UseSeeding((context, _) => DevelopmentSeeder.Seed(context));
                options.UseAsyncSeeding((context, _, ct) => DevelopmentSeeder.SeedAsync(context, ct));
            }
        });

        services.AddScoped<IExhibitionRepository, ExhibitionRepository>();
        services.AddScoped<IExhibitionVersionRepository, ExhibitionVersionRepository>();
        services.AddScoped<IDraftRepository, DraftRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGuestNumberGenerator, GuestNumberGenerator>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();
        await db.Database.MigrateAsync();
    }
}
