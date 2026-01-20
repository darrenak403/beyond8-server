using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDateOfBirthFieldForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");
        }
    }
}
