using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserSubscriptions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "RemainingRequestsPerWeek",
                table: "UserSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestLimitedEndsAt",
                table: "UserSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRemainingRequests",
                table: "UserSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingRequestsPerWeek",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "RequestLimitedEndsAt",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "TotalRemainingRequests",
                table: "UserSubscriptions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserSubscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
