using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeIsActiveFieldByUsingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
