namespace Oekolariet.Application.Common;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
