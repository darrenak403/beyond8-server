using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlatformWalletPendingAndAvailableAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableAt",
                table: "PlatformWalletTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingBalance",
                table: "PlatformWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers",
                column: "AvailableAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_Status_AvailableAt",
                table: "TransactionLedgers",
                columns: new[] { "Status", "AvailableAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsSettled_SettlementEligibleAt",
                table: "Orders",
                columns: new[] { "IsSettled", "SettlementEligibleAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders",
                column: "SettlementEligibleAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_Status_AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsSettled_SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AvailableAt",
                table: "PlatformWalletTransactions");

            migrationBuilder.DropColumn(
                name: "PendingBalance",
                table: "PlatformWallets");
        }
    }
}
