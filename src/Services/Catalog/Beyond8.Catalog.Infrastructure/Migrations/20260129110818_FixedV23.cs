using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedV23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "Sections",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Sections");
        }
    }
}
