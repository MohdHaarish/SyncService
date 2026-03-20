using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncService.Migrations
{
    public partial class AddDateTimeFieldsForEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimestampDateTime",
                table: "CallLogs",
                type: "datetime(0)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSentDateTime",
                table: "Messages",
                type: "datetime(0)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "PostTimeDateTime",
                table: "AppNotifications",
                type: "datetime(0)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenDateTime",
                table: "DeviceIdentifiers",
                type: "datetime(0)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            // Populate historical values from existing unix millisecond columns.
            migrationBuilder.Sql("UPDATE CallLogs SET TimestampDateTime = FROM_UNIXTIME(Timestamp / 1000)");
            migrationBuilder.Sql("UPDATE Messages SET DateSentDateTime = FROM_UNIXTIME(DateSent / 1000)");
            migrationBuilder.Sql("UPDATE AppNotifications SET PostTimeDateTime = FROM_UNIXTIME(PostTime / 1000)");
            migrationBuilder.Sql("UPDATE DeviceIdentifiers SET LastSeenDateTime = FROM_UNIXTIME(LastSeen / 1000)");

            // Normalize to seconds precision
            migrationBuilder.Sql("UPDATE CallLogs SET TimestampDateTime = DATE_FORMAT(TimestampDateTime, '%Y-%m-%d %H:%i:%s')");
            migrationBuilder.Sql("UPDATE Messages SET DateSentDateTime = DATE_FORMAT(DateSentDateTime, '%Y-%m-%d %H:%i:%s')");
            migrationBuilder.Sql("UPDATE AppNotifications SET PostTimeDateTime = DATE_FORMAT(PostTimeDateTime, '%Y-%m-%d %H:%i:%s')");
            migrationBuilder.Sql("UPDATE DeviceIdentifiers SET LastSeenDateTime = DATE_FORMAT(LastSeenDateTime, '%Y-%m-%d %H:%i:%s')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimestampDateTime",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "DateSentDateTime",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PostTimeDateTime",
                table: "AppNotifications");

            migrationBuilder.DropColumn(
                name: "LastSeenDateTime",
                table: "DeviceIdentifiers");
        }
    }
}
