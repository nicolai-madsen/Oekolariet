using Microsoft.EntityFrameworkCore;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;
using Oekolariet.Domain.Aggregates.Editors;
using Oekolariet.Infrastructure.Security;

namespace Oekolariet.Infrastructure.Seeding;

internal static class DevelopmentSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var db = (OekolarietDbContext)context;

        await SeedTestEditorAsync(db, ct);

        if (await db.Exhibitions.AnyAsync(e => e.Id == SeedIds.Exhibition, ct)) // The exhibition is already seeded (from an earlier start), so stop here
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

    // A test editor for development, so we can try the login. Never used in production.
    private static async Task SeedTestEditorAsync(OekolarietDbContext db, CancellationToken ct)
    {
        // Already there from an earlier start? Then do nothing.
        if (await db.Editors.AnyAsync(e => e.Id == SeedIds.Editor, ct))
        {
            return;
        }

        // The password is hashed the same way as for real editors.
        // "Udvikling123!" is a test password for development only. Real passwords are never written in code.
        var hasher = new IdentityPasswordHasher();
        var passwordHash = hasher.Hash("Udvikling123!");

        // Goes through the Domain, so the seed follows the same rules as real data
        var editor = Editor.Create(SeedIds.Editor, "admin@oekolariet.test", passwordHash, DateTime.UtcNow);

        db.Editors.Add(editor);
        await db.SaveChangesAsync(ct);
    }
}

