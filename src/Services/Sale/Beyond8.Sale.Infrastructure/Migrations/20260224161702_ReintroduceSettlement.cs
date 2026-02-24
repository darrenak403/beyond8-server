using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReintroduceSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "PendingBalance",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Mark historical orders as settled if they were already effectively credited
            // i.e., orders with PaidAt set and with an existing completed Sale transaction
            migrationBuilder.Sql(@"UPDATE ""Orders"" o
                                SET ""IsSettled"" = true,
                                    ""SettledAt"" = COALESCE(
                                        (SELECT t.""CreatedAt"" FROM ""TransactionLedgers"" t WHERE t.""ReferenceId"" = o.""Id"" AND t.""Type"" = 0 AND t.""Status"" = 1 LIMIT 1),
                                        o.""PaidAt"")
                                WHERE o.""PaidAt"" IS NOT NULL
                                    AND EXISTS (
                                        SELECT 1 FROM ""TransactionLedgers"" t2
                                        WHERE t2.""ReferenceId"" = o.""Id"" AND t2.""Type"" = 0 AND t2.""Status"" = 1
                                    );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "PendingBalance",
                table: "InstructorWallets");
        }
    }
}
