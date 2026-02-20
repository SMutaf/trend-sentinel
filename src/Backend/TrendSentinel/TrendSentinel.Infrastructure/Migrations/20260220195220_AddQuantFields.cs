using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendSentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfidenceScore",
                table: "NewsLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedDirection",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ImpactStrength",
                table: "NewsLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OverextendedRisk",
                table: "NewsLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeHorizon",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "NewsLogs");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "NewsLogs");

            migrationBuilder.DropColumn(
                name: "ExpectedDirection",
                table: "NewsLogs");

            migrationBuilder.DropColumn(
                name: "ImpactStrength",
                table: "NewsLogs");

            migrationBuilder.DropColumn(
                name: "OverextendedRisk",
                table: "NewsLogs");

            migrationBuilder.DropColumn(
                name: "TimeHorizon",
                table: "NewsLogs");
        }
    }
}
