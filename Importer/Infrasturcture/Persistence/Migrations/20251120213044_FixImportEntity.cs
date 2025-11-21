using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrasturcture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixImportEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportRecords",
                table: "ImportRecords");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "ImportRecords",
                newName: "ImportRecord",
                newSchema: "dbo");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "ImportRecord",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                schema: "dbo",
                table: "ImportRecord",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportRecord",
                schema: "dbo",
                table: "ImportRecord",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportRecord",
                schema: "dbo",
                table: "ImportRecord");

            migrationBuilder.RenameTable(
                name: "ImportRecord",
                schema: "dbo",
                newName: "ImportRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ImportRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ImportRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportRecords",
                table: "ImportRecords",
                column: "Id");
        }
    }
}
