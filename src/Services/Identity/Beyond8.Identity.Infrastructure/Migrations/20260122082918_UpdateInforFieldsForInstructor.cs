using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInforFieldsForInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                    "ALTER TABLE \"InstructorProfiles\" ALTER COLUMN \"BankInfo\" TYPE jsonb USING \"BankInfo\"::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "IntroVideoUrl",
                table: "InstructorProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "TeachingLanguages",
                table: "InstructorProfiles",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntroVideoUrl",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "TeachingLanguages",
                table: "InstructorProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "BankInfo",
                table: "InstructorProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
