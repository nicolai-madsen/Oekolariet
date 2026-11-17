using Oekolariet.Domain.Aggregates.Exhibitions;

namespace Oekolariet.Application.Exhibitions;

public interface IExhibitionRepository
{
    Task<Exhibition?> GetByIdAsync(Guid id, CancellationToken ct);
}
