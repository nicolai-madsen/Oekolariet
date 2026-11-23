using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Games;

namespace Oekolariet.Infrastructure.Persistence;

internal sealed class GuestNumberGenerator(OekolarietDbContext db) : IGuestNumberGenerator
{
    public async Task<int> NextAsync(CancellationToken ct)
    {
        // SQL Server gives each guest a unique number, even when two games start at the same time.
        var numbers = await db.Database
            .SqlQuery<int>($"SELECT NEXT VALUE FOR [GuestNumbers] AS [Value]")
            .ToListAsync(ct);

        return numbers.Single();
    }
}
