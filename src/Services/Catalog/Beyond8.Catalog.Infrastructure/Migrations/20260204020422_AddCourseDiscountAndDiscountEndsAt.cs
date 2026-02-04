using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseDiscountAndDiscountEndsAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Courses",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndsAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Courses",
                type: "numeric(5,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DiscountEndsAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Courses");
        }
    }
}
