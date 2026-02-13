using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountBreakdownToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CouponId",
                table: "Orders",
                newName: "SystemCouponId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                newName: "IX_Orders_SystemCouponId");

            migrationBuilder.AddColumn<Guid>(
                name: "InstructorCouponId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InstructorDiscountAmount",
                table: "Orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SystemDiscountAmount",
                table: "Orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_SystemCouponId",
                table: "Orders",
                column: "SystemCouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_SystemCouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InstructorCouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InstructorDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SystemDiscountAmount",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SystemCouponId",
                table: "Orders",
                newName: "CouponId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SystemCouponId",
                table: "Orders",
                newName: "IX_Orders_CouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
