using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalSubTotalToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalSubTotal",
                table: "Orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalSubTotal",
                table: "Orders");
        }
    }
}
