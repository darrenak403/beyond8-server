using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Learning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugInEnrol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Enrollments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Enrollments");
        }
    }
}
