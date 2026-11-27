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

        var pap = new Category(Guid.NewGuid(), "Pap", MediaId: null);
        var plast = new Category(Guid.NewGuid(), "Plast", MediaId: null);

        var content = new ExhibitionContent(
            new Theme("#2E7D32", "#A5D6A7", "K2D", LogoMediaId: null, BackgroundMediaId: null),
            [
                new Page(
                    Guid.NewGuid(),
                    Order: 1,
                    PageLayout.TextOnly,
                    Title: "Velkommen!",
                    Body: "Hej! Jeg hedder Otto Odder. Følg med og lær om noget skrald, dude.",
                    MediaId: null),
            ],
            [
                new Tile(TileType.Hierarchy, "Affaldshierarkiet", MediaId: null),
                new Tile(TileType.Quiz, "Sorteringsquizzen", MediaId: null),
                new Tile(TileType.Journey, "Materialernes rejse", MediaId: null),
                new Tile(TileType.Offer, "RessourceCenter Vejles tilbud", MediaId: null),
            ],
            new SortingGame(
                Title: "Sorter affaldet",
                Intro: "Træk tingene over i den rigtige spand",
                DurationSeconds: 30,
                Categories: [plast, pap],
                Items:
                [
                    new SortingItem(Guid.NewGuid(), "Mælkekarton", MediaId: null, CorrectCategoryId: pap.Id, null),
                    new SortingItem(Guid.NewGuid(), "Flaske", MediaId: null, CorrectCategoryId: plast.Id, null),
                ]));


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

