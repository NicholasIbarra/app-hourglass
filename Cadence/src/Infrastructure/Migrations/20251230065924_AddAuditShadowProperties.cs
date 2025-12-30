using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditShadowProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ScheduleException",
                newName: "ScheduleException",
                newSchema: "dbo");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Schedule",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Schedule",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Schedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "Schedule",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Schedule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Schedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "dbo",
                table: "Schedule",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CalendarEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CalendarEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CalendarEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CalendarEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CalendarEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Calendar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Calendar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Calendar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "Calendar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Calendar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Calendar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "dbo",
                table: "Calendar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "ScheduleException",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "ScheduleException",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "ScheduleException",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "ScheduleException",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "ScheduleException",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "dbo",
                table: "ScheduleException",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "dbo",
                table: "ScheduleException",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleException_EventId",
                schema: "dbo",
                table: "ScheduleException",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleException_CalendarEvents_EventId",
                schema: "dbo",
                table: "ScheduleException",
                column: "EventId",
                principalTable: "CalendarEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleException_CalendarEvents_EventId",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleException_EventId",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "dbo",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "dbo",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "dbo",
                table: "ScheduleException");

            migrationBuilder.RenameTable(
                name: "ScheduleException",
                schema: "dbo",
                newName: "ScheduleException");
        }
    }
}
