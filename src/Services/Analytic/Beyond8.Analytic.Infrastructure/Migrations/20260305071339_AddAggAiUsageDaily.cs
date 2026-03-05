using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAggAiUsageDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggAiUsageDailies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    TotalInputTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalOutputTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalInputCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    TotalOutputCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggAiUsageDailies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggAiUsageDailies_SnapshotDate",
                table: "AggAiUsageDailies",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_AggAiUsageDailies_SnapshotDate_Model_Provider",
                table: "AggAiUsageDailies",
                columns: new[] { "SnapshotDate", "Model", "Provider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggAiUsageDailies");
        }
    }
}
