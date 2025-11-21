using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrasturcture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileReferenceId",
                schema: "dbo",
                table: "ImportRecord",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ImportType",
                schema: "dbo",
                table: "ImportRecord",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OfficeMapping",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OfficeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeMapping", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfficeMapping",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "FileReferenceId",
                schema: "dbo",
                table: "ImportRecord");

            migrationBuilder.DropColumn(
                name: "ImportType",
                schema: "dbo",
                table: "ImportRecord");
        }
    }
}
