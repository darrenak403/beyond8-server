using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggCourseStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TotalStudents = table.Column<int>(type: "integer", nullable: false),
                    TotalCompletedStudents = table.Column<int>(type: "integer", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AvgRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    TotalRatings = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false),
                    AvgWatchTime = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggCourseStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggInstructorRevenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TotalCourses = table.Column<int>(type: "integer", nullable: false),
                    TotalStudents = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPlatformFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalInstructorEarnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPaidOut = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PendingBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AvgCourseRating = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggInstructorRevenues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggLessonPerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false),
                    UniqueViewers = table.Column<int>(type: "integer", nullable: false),
                    TotalCompletions = table.Column<int>(type: "integer", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AvgWatchPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AvgWatchTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    DropOffPoints = table.Column<string>(type: "jsonb", nullable: true),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggLessonPerformances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggSystemOverviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalUsers = table.Column<int>(type: "integer", nullable: false),
                    TotalActiveUsers = table.Column<int>(type: "integer", nullable: false),
                    NewUsersToday = table.Column<int>(type: "integer", nullable: false),
                    TotalInstructors = table.Column<int>(type: "integer", nullable: false),
                    TotalStudents = table.Column<int>(type: "integer", nullable: false),
                    TotalCourses = table.Column<int>(type: "integer", nullable: false),
                    TotalPublishedCourses = table.Column<int>(type: "integer", nullable: false),
                    TotalEnrollments = table.Column<int>(type: "integer", nullable: false),
                    TotalCompletedEnrollments = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPlatformFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalInstructorEarnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AvgCourseCompletionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AvgCourseRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggSystemOverviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggCourseStats_CourseId",
                table: "AggCourseStats",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AggCourseStats_InstructorId",
                table: "AggCourseStats",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_AggCourseStats_IsCurrent",
                table: "AggCourseStats",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_AggCourseStats_SnapshotDate",
                table: "AggCourseStats",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_AggInstructorRevenues_InstructorId",
                table: "AggInstructorRevenues",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_AggInstructorRevenues_IsCurrent",
                table: "AggInstructorRevenues",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_AggInstructorRevenues_SnapshotDate",
                table: "AggInstructorRevenues",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_AggLessonPerformances_CourseId",
                table: "AggLessonPerformances",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AggLessonPerformances_InstructorId",
                table: "AggLessonPerformances",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_AggLessonPerformances_IsCurrent",
                table: "AggLessonPerformances",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_AggLessonPerformances_LessonId",
                table: "AggLessonPerformances",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_AggLessonPerformances_SnapshotDate",
                table: "AggLessonPerformances",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviews_IsCurrent",
                table: "AggSystemOverviews",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviews_SnapshotDate",
                table: "AggSystemOverviews",
                column: "SnapshotDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggCourseStats");

            migrationBuilder.DropTable(
                name: "AggInstructorRevenues");

            migrationBuilder.DropTable(
                name: "AggLessonPerformances");

            migrationBuilder.DropTable(
                name: "AggSystemOverviews");
        }
    }
}
