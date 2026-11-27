using Microsoft.EntityFrameworkCore;
using Oekolariet.Application.Auth;
using Oekolariet.Domain.Aggregates.Editors;

namespace Oekolariet.Infrastructure.Repositories;

internal sealed class EditorRepository(OekolarietDbContext db) : IEditorRepository
{
    // Looks up an editor in the database by email.
    public Task<Editor?> GetByEmailAsync(string email, CancellationToken ct)
    {
        // Emails are saved in lowercase without spaces (see Editor.Create), so we search the same way
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return db.Editors.FirstOrDefaultAsync(e => e.Email == normalizedEmail, ct);
    }
}
