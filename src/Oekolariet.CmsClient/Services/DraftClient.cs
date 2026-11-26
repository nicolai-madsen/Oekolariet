using System.Net;
using System.Net.Http.Json;
using Oekolariet.Shared.Drafts;

namespace Oekolariet.CmsClient.Services;

// Class is the only, in the CMS, that knows the draft URLs
public sealed class DraftClient(HttpClient http)
{
    // Method returns null if an exhibition has no draft (which is a valid state, not an error)
    public async Task<DraftDto?> GetAsync(Guid exhibitionId, CancellationToken ct = default)
    {
        // "using" is IDispoable, so the data is removed from memory, when its done being used.
        using var response = await http.GetAsync($"api/v1/exhibitions/{exhibitionId}/draft", ct); 

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode(); // throw an exception if the response's status code is not a success status code.

        return await response.Content.ReadFromJsonAsync<DraftDto>(ct);
    }

    // POST creates a draft as a copy of the active version (will just be empty, if there isnt a version to copy from)
    public async Task<DraftDto> CreateAsync(Guid exhibitionId, CancellationToken ct = default) // return type is not nullable here, cuz' a succesfull POST can ONLY return a draft.
    {
        using var response = await http.PostAsync($"api/v1/exhibitions/{exhibitionId}/draft", content: null, ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DraftDto>(ct) ?? throw new InvalidOperationException("The API returrned 201 without a draft.");
    }
}
