using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendSentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_NewsLog_ValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "NewsLogs");

            migrationBuilder.RenameColumn(
                name: "TrendSummary",
                table: "NewsLogs",
                newName: "Analysis_TrendSummary");

            migrationBuilder.RenameColumn(
                name: "TimeHorizon",
                table: "NewsLogs",
                newName: "Quant_TimeHorizon");

            migrationBuilder.RenameColumn(
                name: "OverextendedRisk",
                table: "NewsLogs",
                newName: "Quant_OverextendedRisk");

            migrationBuilder.RenameColumn(
                name: "IsTrendTriggered",
                table: "NewsLogs",
                newName: "Analysis_IsTrendTriggered");

            migrationBuilder.RenameColumn(
                name: "ImpactStrength",
                table: "NewsLogs",
                newName: "Quant_ImpactStrength");

            migrationBuilder.RenameColumn(
                name: "ExpectedDirection",
                table: "NewsLogs",
                newName: "Quant_ExpectedDirection");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "NewsLogs",
                newName: "Quant_EventType");

            migrationBuilder.RenameColumn(
                name: "ConfidenceScore",
                table: "NewsLogs",
                newName: "Analysis_ConfidenceScore");

            migrationBuilder.AlterColumn<string>(
                name: "Analysis_TrendSummary",
                table: "NewsLogs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Quant_TimeHorizon",
                table: "NewsLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "Quant_OverextendedRisk",
                table: "NewsLogs",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Analysis_IsTrendTriggered",
                table: "NewsLogs",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "Quant_ImpactStrength",
                table: "NewsLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Quant_ExpectedDirection",
                table: "NewsLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Quant_EventType",
                table: "NewsLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Analysis_ConfidenceScore",
                table: "NewsLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Analysis_Sentiment",
                table: "NewsLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Analysis_Sentiment",
                table: "NewsLogs");

            migrationBuilder.RenameColumn(
                name: "Quant_TimeHorizon",
                table: "NewsLogs",
                newName: "TimeHorizon");

            migrationBuilder.RenameColumn(
                name: "Quant_OverextendedRisk",
                table: "NewsLogs",
                newName: "OverextendedRisk");

            migrationBuilder.RenameColumn(
                name: "Quant_ImpactStrength",
                table: "NewsLogs",
                newName: "ImpactStrength");

            migrationBuilder.RenameColumn(
                name: "Quant_ExpectedDirection",
                table: "NewsLogs",
                newName: "ExpectedDirection");

            migrationBuilder.RenameColumn(
                name: "Quant_EventType",
                table: "NewsLogs",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "Analysis_TrendSummary",
                table: "NewsLogs",
                newName: "TrendSummary");

            migrationBuilder.RenameColumn(
                name: "Analysis_IsTrendTriggered",
                table: "NewsLogs",
                newName: "IsTrendTriggered");

            migrationBuilder.RenameColumn(
                name: "Analysis_ConfidenceScore",
                table: "NewsLogs",
                newName: "ConfidenceScore");

            migrationBuilder.AlterColumn<string>(
                name: "TimeHorizon",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "OverextendedRisk",
                table: "NewsLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ImpactStrength",
                table: "NewsLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedDirection",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrendSummary",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrendTriggered",
                table: "NewsLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ConfidenceScore",
                table: "NewsLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "NewsLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
