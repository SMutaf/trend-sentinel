using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendSentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTechnicalSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_Companies_CompanyId",
                table: "PriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_CompanyId",
                table: "PriceHistories");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "PriceHistories",
                newName: "NewsLogId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_NewsLogId",
                table: "PriceHistories",
                column: "NewsLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTechnicalSnapshots_NewsLogId",
                table: "EventTechnicalSnapshots",
                column: "NewsLogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_NewsLogs_NewsLogId",
                table: "PriceHistories",
                column: "NewsLogId",
                principalTable: "NewsLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_NewsLogs_NewsLogId",
                table: "PriceHistories");

            migrationBuilder.DropTable(
                name: "EventTechnicalSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_NewsLogId",
                table: "PriceHistories");

            migrationBuilder.RenameColumn(
                name: "NewsLogId",
                table: "PriceHistories",
                newName: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_CompanyId",
                table: "PriceHistories",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_Companies_CompanyId",
                table: "PriceHistories",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
