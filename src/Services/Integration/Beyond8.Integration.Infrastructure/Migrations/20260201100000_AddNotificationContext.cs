using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Integration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Context",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0); // 0 = General
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Notifications");
        }
    }
}
