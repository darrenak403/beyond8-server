using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "CartItems",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndsAt",
                table: "CartItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "CartItems",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "CartItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "DiscountEndsAt",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "CartItems");
        }
    }
}
