using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSaleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayoutRequests_InstructorWallets_InstructorWalletId",
                table: "PayoutRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLedgers_InstructorWallets_InstructorWalletId",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_InstructorWalletId",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "TransactionLedgers");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "TransactionLedgers",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "InstructorWalletId",
                table: "TransactionLedgers",
                newName: "WalletId");

            migrationBuilder.RenameColumn(
                name: "InstructorWalletId",
                table: "PayoutRequests",
                newName: "WalletId");

            migrationBuilder.RenameColumn(
                name: "BankAccountInfo",
                table: "PayoutRequests",
                newName: "RejectionReason");

            migrationBuilder.RenameIndex(
                name: "IX_PayoutRequests_InstructorWalletId",
                table: "PayoutRequests",
                newName: "IX_PayoutRequests_WalletId");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableAt",
                table: "TransactionLedgers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "TransactionLedgers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBefore",
                table: "TransactionLedgers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TransactionLedgers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalTransactionId",
                table: "TransactionLedgers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "TransactionLedgers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "TransactionLedgers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PayoutRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "PayoutRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "PayoutRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "PayoutRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "PayoutRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PayoutRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalTransactionId",
                table: "PayoutRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "PayoutRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "PayoutRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestNumber",
                table: "PayoutRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Payments",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Orders",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDetails",
                table: "Orders",
                type: "jsonb",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseThumbnail",
                table: "OrderItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "OrderItems",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstructorEarnings",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "OrderItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeeAmount",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeePercent",
                table: "OrderItems",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountInfo",
                table: "InstructorWallets",
                type: "jsonb",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HoldBalance",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InstructorWallets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPayoutAt",
                table: "InstructorWallets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEarnings",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWithdrawn",
                table: "InstructorWallets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "CouponUsages",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Coupons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicableCourseId",
                table: "Coupons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicableInstructorId",
                table: "Coupons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsagePerUser",
                table: "Coupons",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers",
                column: "AvailableAt",
                filter: "\"Status\" = 0 AND \"AvailableAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_ReferenceId_ReferenceType",
                table: "TransactionLedgers",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_Status",
                table: "TransactionLedgers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_Type",
                table: "TransactionLedgers",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_WalletId_CreatedAt",
                table: "TransactionLedgers",
                columns: new[] { "WalletId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_InstructorId",
                table: "PayoutRequests",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_RequestedAt",
                table: "PayoutRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_RequestNumber",
                table: "PayoutRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_Status",
                table: "PayoutRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_Status_RequestedAt",
                table: "PayoutRequests",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalTransactionId",
                table: "Payments",
                column: "ExternalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaidAt",
                table: "Payments",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNumber",
                table: "Payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Provider_Status",
                table: "Payments",
                columns: new[] { "Provider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaidAt",
                table: "Orders",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders",
                column: "SettlementEligibleAt",
                filter: "\"IsSettled\" = false AND \"SettlementEligibleAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_PaidAt",
                table: "Orders",
                columns: new[] { "Status", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CourseId",
                table: "OrderItems",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InstructorId",
                table: "OrderItems",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorWallets_IsActive_AvailableBalance",
                table: "InstructorWallets",
                columns: new[] { "IsActive", "AvailableBalance" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId_UserId",
                table: "CouponUsages",
                columns: new[] { "CouponId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ApplicableCourseId",
                table: "Coupons",
                column: "ApplicableCourseId",
                filter: "\"ApplicableCourseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ApplicableInstructorId",
                table: "Coupons",
                column: "ApplicableInstructorId",
                filter: "\"ApplicableInstructorId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive_ValidFrom_ValidTo",
                table: "Coupons",
                columns: new[] { "IsActive", "ValidFrom", "ValidTo" });

            migrationBuilder.AddForeignKey(
                name: "FK_PayoutRequests_InstructorWallets_WalletId",
                table: "PayoutRequests",
                column: "WalletId",
                principalTable: "InstructorWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLedgers_InstructorWallets_WalletId",
                table: "TransactionLedgers",
                column: "WalletId",
                principalTable: "InstructorWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayoutRequests_InstructorWallets_WalletId",
                table: "PayoutRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLedgers_InstructorWallets_WalletId",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_ReferenceId_ReferenceType",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_Status",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_Type",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_TransactionLedgers_WalletId_CreatedAt",
                table: "TransactionLedgers");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_InstructorId",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_RequestedAt",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_RequestNumber",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_Status",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_Status_RequestedAt",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ExternalTransactionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaidAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentNumber",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Provider_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaidAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_PaidAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_CourseId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_InstructorId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_InstructorWallets_IsActive_AvailableBalance",
                table: "InstructorWallets");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_CouponId_UserId",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_ApplicableCourseId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_ApplicableInstructorId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_IsActive_ValidFrom_ValidTo",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "AvailableAt",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "ExternalTransactionId",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "TransactionLedgers");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "ExternalTransactionId",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "RequestNumber",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentDetails",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SettlementEligibleAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CourseThumbnail",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "InstructorEarnings",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PlatformFeeAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PlatformFeePercent",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BankAccountInfo",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "HoldBalance",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "LastPayoutAt",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "TotalEarnings",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "TotalWithdrawn",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "CouponUsages");

            migrationBuilder.DropColumn(
                name: "ApplicableCourseId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "ApplicableInstructorId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UsagePerUser",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "TransactionLedgers",
                newName: "InstructorWalletId");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "TransactionLedgers",
                newName: "OrderItemId");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "PayoutRequests",
                newName: "InstructorWalletId");

            migrationBuilder.RenameColumn(
                name: "RejectionReason",
                table: "PayoutRequests",
                newName: "BankAccountInfo");

            migrationBuilder.RenameIndex(
                name: "IX_PayoutRequests_WalletId",
                table: "PayoutRequests",
                newName: "IX_PayoutRequests_InstructorWalletId");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "TransactionLedgers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Coupons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_InstructorWalletId",
                table: "TransactionLedgers",
                column: "InstructorWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayoutRequests_InstructorWallets_InstructorWalletId",
                table: "PayoutRequests",
                column: "InstructorWalletId",
                principalTable: "InstructorWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLedgers_InstructorWallets_InstructorWalletId",
                table: "TransactionLedgers",
                column: "InstructorWalletId",
                principalTable: "InstructorWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
