using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHlsVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoHlsUrl",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "HlsVariants",
                table: "Lessons",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HlsVariants",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "VideoHlsUrl",
                table: "Lessons",
                type: "text",
                nullable: true);
        }
    }
}
