using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Assessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReassignRequestAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReassignHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResetByInstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedCount = table.Column<int>(type: "integer", nullable: false),
                    ReassignRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReassignHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReassignRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReassignRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReassignHistories_ResetAt",
                table: "ReassignHistories",
                column: "ResetAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReassignHistories_StudentId",
                table: "ReassignHistories",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReassignHistories_Type_SourceId",
                table: "ReassignHistories",
                columns: new[] { "Type", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReassignRequests_Status",
                table: "ReassignRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReassignRequests_StudentId",
                table: "ReassignRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReassignRequests_Type_SourceId",
                table: "ReassignRequests",
                columns: new[] { "Type", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReassignRequests_Type_SourceId_StudentId",
                table: "ReassignRequests",
                columns: new[] { "Type", "SourceId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReassignHistories");

            migrationBuilder.DropTable(
                name: "ReassignRequests");
        }
    }
}
