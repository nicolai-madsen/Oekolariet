using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oekolariet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "GuestNumbers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "GuestNumbers");
        }
    }
}
