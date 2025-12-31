using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForiegnKeyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Calendar_CalendarId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Schedule_ScheduleId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Calendar_CalendarId",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleException_Schedule_ScheduleId",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Calendar_CalendarId",
                table: "CalendarEvents",
                column: "CalendarId",
                principalSchema: "dbo",
                principalTable: "Calendar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Schedule_ScheduleId",
                table: "CalendarEvents",
                column: "ScheduleId",
                principalSchema: "dbo",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Calendar_CalendarId",
                schema: "dbo",
                table: "Schedule",
                column: "CalendarId",
                principalSchema: "dbo",
                principalTable: "Calendar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleException_Schedule_ScheduleId",
                schema: "dbo",
                table: "ScheduleException",
                column: "ScheduleId",
                principalSchema: "dbo",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Calendar_CalendarId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Schedule_ScheduleId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Calendar_CalendarId",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleException_Schedule_ScheduleId",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Calendar_CalendarId",
                table: "CalendarEvents",
                column: "CalendarId",
                principalSchema: "dbo",
                principalTable: "Calendar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Schedule_ScheduleId",
                table: "CalendarEvents",
                column: "ScheduleId",
                principalSchema: "dbo",
                principalTable: "Schedule",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Calendar_CalendarId",
                schema: "dbo",
                table: "Schedule",
                column: "CalendarId",
                principalSchema: "dbo",
                principalTable: "Calendar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleException_Schedule_ScheduleId",
                schema: "dbo",
                table: "ScheduleException",
                column: "ScheduleId",
                principalSchema: "dbo",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
