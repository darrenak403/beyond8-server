using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveAssignmentIdFromLessonToSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add AssignmentId to Sections table
            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "Sections",
                type: "uuid",
                nullable: true);

            // Drop AssignmentId from Lessons table
            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Lessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore AssignmentId to Lessons table
            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "Lessons",
                type: "uuid",
                nullable: true);

            // Remove AssignmentId from Sections table
            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Sections");
        }
    }
}
