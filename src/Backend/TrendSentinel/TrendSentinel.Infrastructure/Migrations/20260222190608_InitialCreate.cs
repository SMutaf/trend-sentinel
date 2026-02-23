using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendSentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TickerSymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Sector = table.Column<int>(type: "integer", nullable: false),
                    IsAlertSent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    IsTrendTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    TrendSummary = table.Column<string>(type: "text", nullable: false),
                    SentimentLabel = table.Column<string>(type: "text", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ImpactStrength = table.Column<int>(type: "integer", nullable: false),
                    ExpectedDirection = table.Column<string>(type: "text", nullable: false),
                    TimeHorizon = table.Column<string>(type: "text", nullable: false),
                    OverextendedRisk = table.Column<bool>(type: "boolean", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsLogs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTechnicalSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RsiValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MacdState = table.Column<string>(type: "text", nullable: false),
                    TechScore = table.Column<int>(type: "integer", nullable: false),
                    IsOverextended = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTechnicalSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTechnicalSnapshots_NewsLogs_NewsLogId",
                        column: x => x.NewsLogId,
                        principalTable: "NewsLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Open = table.Column<decimal>(type: "numeric", nullable: false),
                    High = table.Column<decimal>(type: "numeric", nullable: false),
                    Low = table.Column<decimal>(type: "numeric", nullable: false),
                    Close = table.Column<decimal>(type: "numeric", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_NewsLogs_NewsLogId",
                        column: x => x.NewsLogId,
                        principalTable: "NewsLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTechnicalSnapshots_NewsLogId",
                table: "EventTechnicalSnapshots",
                column: "NewsLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsLogs_CompanyId",
                table: "NewsLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_NewsLogId",
                table: "PriceHistories",
                column: "NewsLogId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTechnicalSnapshots");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "NewsLogs");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
