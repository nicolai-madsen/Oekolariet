using Oekolariet.Domain.Aggregates.Editors;

namespace Oekolariet.Application.Auth;

// Finds editors in the database. Application only knows this promise.
public interface IEditorRepository
{
    // Returns the editor with this email, or null if there is none
    Task<Editor?> GetByEmailAsync(string email, CancellationToken ct);
}
