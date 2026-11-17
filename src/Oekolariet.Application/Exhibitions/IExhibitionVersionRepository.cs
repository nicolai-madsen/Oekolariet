using Oekolariet.Domain.Aggregates.ExhibitionVersions;

namespace Oekolariet.Application.Exhibitions;

public interface IExhibitionVersionRepository
{
    Task<ExhibitionVersion?> GetByIdAsync(Guid id, CancellationToken ct);
}
