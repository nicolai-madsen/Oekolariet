using Microsoft.EntityFrameworkCore;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;

namespace Oekolariet.Infrastructure.Seeding;

internal static class DevelopmentSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var db = (OekolarietDbContext)context;

        if (await db.Exhibitions.AnyAsync(e => e.Id == SeedIds.Exhibition, ct))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var exhibition = Exhibition.Create(SeedIds.Exhibition, "Økolariet", now);

        var content = new ExhibitionContent(
            new Theme("#2E7D32", "#A5D6A7", "K2D", LogoMediaId: null, BackgroundMediaId: null),
            [
                new Page(
                    Guid.NewGuid(),
                    Order: 1,
                    PageLayout.TextOnly,
                    Title: "Velkommen!",
                    Body: "Hej! Jeg hedder Otto Odder. Kom med mig rundt i udstillingen og lær, hvordan affald kan blive til nye ting.",
                    MediaId: null),
            ]);

        // Går gennem domænet, så seed-data overholder samme regler som rigtige data.
        var version = exhibition.Publish(content, "seed@oekolariet.local", now);

        db.Exhibitions.Add(exhibition);
        db.ExhibitionVersions.Add(version);
        await db.SaveChangesAsync(ct);
    }

    public static void Seed(DbContext context)
    {
        SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();
    }
}

