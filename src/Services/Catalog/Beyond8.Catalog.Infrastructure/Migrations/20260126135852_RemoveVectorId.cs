using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVectorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VectorDbId",
                table: "LessonDocuments");

            migrationBuilder.DropColumn(
                name: "VectorDbId",
                table: "CourseDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VectorDbId",
                table: "LessonDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VectorDbId",
                table: "CourseDocuments",
                type: "uuid",
                nullable: true);
        }
    }
}
