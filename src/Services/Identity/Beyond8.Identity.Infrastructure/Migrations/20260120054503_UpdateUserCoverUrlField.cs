using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserCoverUrlField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Users");
        }
    }
}
