using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuizFieldsFromLessonQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinCompletionSeconds",
                table: "LessonQuizzes");

            migrationBuilder.DropColumn(
                name: "RequiredScore",
                table: "LessonQuizzes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinCompletionSeconds",
                table: "LessonQuizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredScore",
                table: "LessonQuizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
