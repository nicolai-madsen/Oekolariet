using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Drafts;
using Oekolariet.Domain.Aggregates.Drafts;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class DraftRepository(OekolarietDbContext db) : IDraftRepository
{
    public Task<Draft?> GetByExhibitionIdAsync(Guid exhibitionId, CancellationToken ct)
    {
        return db.Drafts.FirstOrDefaultAsync(d => d.ExhibitionId == exhibitionId, ct);
    }

    public void Add(Draft draft)
    {
        db.Drafts.Add(draft);
    }
}
