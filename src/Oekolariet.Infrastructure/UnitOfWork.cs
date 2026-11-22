using Oekolariet.Application.Common;

namespace Oekolariet.Infrastructure;

internal sealed class UnitOfWork(OekolarietDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }
}
