using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oekolariet.Domain.Aggregates.Editors;

namespace Oekolariet.Infrastructure.Configurations;

// How an Editor is stored in the database
internal sealed class EditorConfiguration : IEntityTypeConfiguration<Editor>
{
    public void Configure(EntityTypeBuilder<Editor> builder)
    {
        builder.ToTable("Editors");
        builder.HasKey(e => e.Id); // primary key: each editor's unique id
        builder.Property(e => e.Id).ValueGeneratedNever(); // the id comes from Editor.Create, not from SQL Server
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(e => e.Email).IsUnique(); // two editors cannot have the same email
        builder.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
    }
}
