using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Domain.Aggregates.Exhibitions;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class ExhibitionRepository(OekolarietDbContext db) : IExhibitionRepository
{
    public Task<Exhibition?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return db.Exhibitions.FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
