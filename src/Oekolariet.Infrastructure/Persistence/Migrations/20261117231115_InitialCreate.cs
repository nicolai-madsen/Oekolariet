using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oekolariet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exhibitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ActiveVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LatestVersionNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exhibitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitionVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExhibitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExhibitionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExhibitionVersions_Exhibitions_ExhibitionId",
                        column: x => x.ExhibitionId,
                        principalTable: "Exhibitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitionVersions_ExhibitionId_VersionNumber",
                table: "ExhibitionVersions",
                columns: new[] { "ExhibitionId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExhibitionVersions");

            migrationBuilder.DropTable(
                name: "Exhibitions");
        }
    }
}
