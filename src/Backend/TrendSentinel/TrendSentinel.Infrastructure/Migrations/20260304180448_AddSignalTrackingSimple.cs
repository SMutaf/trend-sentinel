using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendSentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignalTrackingSimple : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalTrack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    PerformancePercent = table.Column<decimal>(type: "numeric", nullable: true),
                    DaysElapsed = table.Column<int>(type: "integer", nullable: false),
                    TargetDurationDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxPerformancePercent = table.Column<decimal>(type: "numeric", nullable: true),
                    PeakDay = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignalTrack_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignalTrack_NewsLogs_NewsLogId",
                        column: x => x.NewsLogId,
                        principalTable: "NewsLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignalPricePoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignalTrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Open = table.Column<decimal>(type: "numeric", nullable: false),
                    High = table.Column<decimal>(type: "numeric", nullable: false),
                    Low = table.Column<decimal>(type: "numeric", nullable: false),
                    Close = table.Column<decimal>(type: "numeric", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    CumulativeChangePercent = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalPricePoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignalPricePoint_SignalTrack_SignalTrackId",
                        column: x => x.SignalTrackId,
                        principalTable: "SignalTrack",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalPricePoint_SignalTrackId_DayNumber",
                table: "SignalPricePoint",
                columns: new[] { "SignalTrackId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalTrack_CompanyId",
                table: "SignalTrack",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalTrack_NewsLogId",
                table: "SignalTrack",
                column: "NewsLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalTrack_Status_EntryDate",
                table: "SignalTrack",
                columns: new[] { "Status", "EntryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalPricePoint");

            migrationBuilder.DropTable(
                name: "SignalTrack");
        }
    }
}
