using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oekolariet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the keys already stored on games so existing scores show the correct avatar.
            migrationBuilder.InsertData(
                table: "Avatars",
                columns: new[] { "Key", "Name", "ImageUrl", "IsActive" },
                values: new object[,]
                {
            { "odder", "Otto Odder", "images/Avatars/otto-default.png", true },
            { "egern", "Egern", "images/Avatars/EgernAvatar.png", true },
            { "froe", "Frø", "images/Avatars/FroeAvatar.png", true },
            { "graevling", "Grævling", "images/Avatars/GraevlingAvatar.png", true },
            { "pindsvin", "Pindsvin", "images/Avatars/PindsvinAvatar.png", true }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var key in new[] { "odder", "egern", "froe", "graevling", "pindsvin" })
            {
                migrationBuilder.DeleteData(
                    table: "Avatars",
                    keyColumn: "Key",
                    keyValue: key);
            }
        }
    }
}
