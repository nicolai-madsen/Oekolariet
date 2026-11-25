using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Drafts;
using Oekolariet.Shared.Drafts;

namespace Okelariet.Application.Drafts;

// Handler for retrieving a draft associated with a specific exhibition.
// It uses the IDraftRepository to fetch the draft and maps it to a DraftDto for returning to the caller.
public sealed class GetDraftHandler(IDraftRepository drafts)
{
    public async Task<DraftDto?> HandleAsync(Guid exhibitionId, CancellationToken ct)
    {
        var draft = await drafts.GetByExhibitionIdAsync(exhibitionId, ct);
        if (draft is null)
        {
            throw new NotFoundException("Kladde", exhibitionId);
        }
        return draft.ToDto(); // From DraftMappings.
    }
}
