using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCalendarEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Calendar_CalendarId",
                        column: x => x.CalendarId,
                        principalSchema: "dbo",
                        principalTable: "Calendar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CalendarId",
                table: "CalendarEvents",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_ScheduleId",
                table: "CalendarEvents",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEvents");
        }
    }
}
