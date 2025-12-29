using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedule",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    RecurrencePattern_DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    Month = table.Column<int>(type: "int", nullable: true),
                    OccurrenceCount = table.Column<int>(type: "int", nullable: true),
                    RecurrenceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedule_Calendar_CalendarId",
                        column: x => x.CalendarId,
                        principalSchema: "dbo",
                        principalTable: "Calendar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleException",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExceptionType = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleException", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleException_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_CalendarId",
                schema: "dbo",
                table: "Schedule",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleException_ScheduleId",
                table: "ScheduleException",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleException");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "dbo");
        }
    }
}
