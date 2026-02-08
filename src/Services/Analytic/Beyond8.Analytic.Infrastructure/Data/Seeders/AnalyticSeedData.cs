using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Data.Seeders;

/// <summary>
/// Seed data for Analytic service, aligned with Catalog/Learning/Assessment seed GUIDs.
/// </summary>
public static class AnalyticSeedData
{
    // Align with CatalogSeedData / LearningSeedData / AssessmentSeedData
    private static readonly Guid SeedCourseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    private const string InstructorName = "Trần Thị Giảng Viên";
    private const string CourseTitle = "Lập trình ASP.NET Core từ cơ bản đến nâng cao";

    private static readonly Guid Lesson1_1Id = Guid.Parse("55555555-5555-5555-5555-555555550101");
    private static readonly Guid Lesson1_2Id = Guid.Parse("55555555-5555-5555-5555-555555550102");
    private static readonly Guid Lesson1_3Id = Guid.Parse("55555555-5555-5555-5555-555555550103");
    private static readonly Guid Lesson2_1Id = Guid.Parse("55555555-5555-5555-5555-555555550201");
    private static readonly Guid Lesson2_2Id = Guid.Parse("55555555-5555-5555-5555-555555550202");
    private static readonly Guid Lesson2_3Id = Guid.Parse("55555555-5555-5555-5555-555555550203");
    private static readonly Guid Lesson2_4Id = Guid.Parse("55555555-5555-5555-5555-555555550204");
    private static readonly Guid Lesson3_1Id = Guid.Parse("55555555-5555-5555-5555-555555550301");
    private static readonly Guid Lesson3_2Id = Guid.Parse("55555555-5555-5555-5555-555555550302");
    private static readonly Guid Lesson3_3Id = Guid.Parse("55555555-5555-5555-5555-555555550303");

    private static readonly Guid SeedSystemOverviewId = Guid.Parse("88888888-8888-8888-8888-888888888801");
    private static readonly Guid SeedCourseStatsId = Guid.Parse("88888888-8888-8888-8888-888888888802");
    private static readonly Guid SeedInstructorRevenueId = Guid.Parse("88888888-8888-8888-8888-888888888803");
    private static readonly Guid[] SeedLessonPerformanceIds =
    {
        Guid.Parse("88888888-8888-8888-8888-888888888811"),
        Guid.Parse("88888888-8888-8888-8888-888888888812"),
        Guid.Parse("88888888-8888-8888-8888-888888888813"),
        Guid.Parse("88888888-8888-8888-8888-888888888814"),
        Guid.Parse("88888888-8888-8888-8888-888888888815"),
        Guid.Parse("88888888-8888-8888-8888-888888888816"),
        Guid.Parse("88888888-8888-8888-8888-888888888817"),
        Guid.Parse("88888888-8888-8888-8888-888888888818"),
        Guid.Parse("88888888-8888-8888-8888-888888888819"),
        Guid.Parse("88888888-8888-8888-8888-88888888880a")
    };

    public static async Task SeedAsync(AnalyticDbContext context)
    {
        if (await context.AggSystemOverviews.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var snapshotDate = DateOnly.FromDateTime(now);

        // 1. AggSystemOverview (1 course published, 1 enrollment from Learning seed)
        await context.AggSystemOverviews.AddAsync(new AggSystemOverview
        {
            Id = SeedSystemOverviewId,
            TotalUsers = 0,
            TotalActiveUsers = 0,
            NewUsersToday = 0,
            TotalInstructors = 1,
            TotalStudents = 1,
            TotalCourses = 1,
            TotalPublishedCourses = 1,
            TotalEnrollments = 1,
            TotalCompletedEnrollments = 1,
            TotalRevenue = 0,
            TotalPlatformFee = 0,
            TotalInstructorEarnings = 0,
            TotalRefundAmount = 0,
            AvgCourseCompletionRate = 100,
            AvgCourseRating = 0,
            TotalReviews = 0,
            SnapshotDate = snapshotDate,
            IsCurrent = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        });

        // 2. AggCourseStats (seed course)
        await context.AggCourseStats.AddAsync(new AggCourseStats
        {
            Id = SeedCourseStatsId,
            CourseId = SeedCourseId,
            CourseTitle = CourseTitle,
            InstructorId = SeedInstructorId,
            InstructorName = InstructorName,
            TotalStudents = 1,
            TotalCompletedStudents = 1,
            CompletionRate = 100,
            AvgRating = null,
            TotalReviews = 0,
            TotalRatings = 0,
            TotalRevenue = 0,
            TotalRefundAmount = 0,
            NetRevenue = 0,
            TotalViews = 0,
            AvgWatchTime = 0,
            SnapshotDate = snapshotDate,
            IsCurrent = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        });

        // 3. AggInstructorRevenue (seed instructor)
        await context.AggInstructorRevenues.AddAsync(new AggInstructorRevenue
        {
            Id = SeedInstructorRevenueId,
            InstructorId = SeedInstructorId,
            InstructorName = InstructorName,
            TotalCourses = 1,
            TotalStudents = 1,
            TotalRevenue = 0,
            TotalPlatformFee = 0,
            TotalInstructorEarnings = 0,
            TotalRefundAmount = 0,
            TotalPaidOut = 0,
            PendingBalance = 0,
            AvgCourseRating = 0,
            TotalReviews = 0,
            SnapshotDate = snapshotDate,
            IsCurrent = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        });

        // 4. AggLessonPerformance (lessons of seed course - align with Catalog lesson titles)
        var lessons = new[]
        {
            (Lesson1_1Id, "Giới thiệu khóa học và lộ trình học tập"),
            (Lesson1_2Id, "Cài đặt môi trường phát triển"),
            (Lesson1_3Id, "Cấu trúc project ASP.NET Core"),
            (Lesson2_1Id, "Dependency Injection trong ASP.NET Core"),
            (Lesson2_2Id, "Middleware Pipeline"),
            (Lesson2_3Id, "Configuration và Options Pattern"),
            (Lesson2_4Id, "Quiz: Kiểm tra kiến thức Section 2"),
            (Lesson3_1Id, "Giới thiệu Entity Framework Core"),
            (Lesson3_2Id, "Migrations và Database Management"),
            (Lesson3_3Id, "Quiz: Entity Framework Core")
        };

        for (var i = 0; i < lessons.Length; i++)
        {
            var (lessonId, title) = lessons[i];
            await context.AggLessonPerformances.AddAsync(new AggLessonPerformance
            {
                Id = SeedLessonPerformanceIds[i],
                LessonId = lessonId,
                LessonTitle = title,
                CourseId = SeedCourseId,
                InstructorId = SeedInstructorId,
                TotalViews = 0,
                UniqueViewers = 0,
                TotalCompletions = 0,
                CompletionRate = 0,
                AvgWatchPercent = 0,
                AvgWatchTimeSeconds = 0,
                SnapshotDate = snapshotDate,
                IsCurrent = true,
                CreatedAt = now,
                CreatedBy = SeedInstructorId
            });
        }

        await context.SaveChangesAsync();
    }
}
