using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HoldBalance",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "PendingBalance",
                table: "InstructorWallets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableAt",
                table: "TransactionLedgers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettlementEligibleAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HoldBalance",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingBalance",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers",
                column: "AvailableAt",
                filter: "\"Status\" = 0 AND \"AvailableAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders",
                column: "SettlementEligibleAt",
                filter: "\"IsSettled\" = false AND \"SettlementEligibleAt\" IS NOT NULL");
        }
    }
}
