using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.ExhibitionVersions;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class ExhibitionVersionRepository(OekolarietDbContext db) : IExhibitionVersionRepository
{
    public Task<ExhibitionVersion?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // Versioner ændres aldrig, så ingen grund til change tracking.
        return db.ExhibitionVersions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
    }
}
