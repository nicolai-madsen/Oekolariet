using Oekolariet.Domain.Aggregates.Drafts;

namespace Oekolariet.Application.Drafts;

public interface IDraftRepository
{
    Task<Draft?> GetByExhibitionIdAsync(Guid exhibitionId, CancellationToken ct);
    void Add(Draft draft);
}
