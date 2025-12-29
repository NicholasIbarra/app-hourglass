using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecurrencePattern_DayOfMonth",
                schema: "dbo",
                table: "Schedule",
                newName: "DayOfMonth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DayOfMonth",
                schema: "dbo",
                table: "Schedule",
                newName: "RecurrencePattern_DayOfMonth");
        }
    }
}
