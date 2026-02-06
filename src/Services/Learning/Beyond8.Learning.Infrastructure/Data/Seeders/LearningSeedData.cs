using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Enums;
using Beyond8.Learning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Learning.Infrastructure.Data.Seeders;

public static class LearningSeedData
{
    private static readonly Guid SeedStudentId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    private static readonly Guid SeedInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    private static readonly Guid SeedCourseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedEnrollment1Id = Guid.Parse("77777777-7777-7777-7777-777777777701");
    private static readonly Guid SeedCertificate1Id = Guid.Parse("77777777-7777-7777-7777-777777777702");

    private static readonly Guid Section1Id = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private static readonly Guid Section2Id = Guid.Parse("44444444-4444-4444-4444-444444444402");
    private static readonly Guid Section3Id = Guid.Parse("44444444-4444-4444-4444-444444444403");

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

    private const string StudentFullName = "Nguyễn Văn Học";
    private const string InstructorName = "Trần Thị Giảng Viên";
    private const string SeedImageUrl = "https://d30z0qh7rhzgt8.cloudfront.net/course/thumbnails/00000000-0000-0000-0000-000000000006/7657afd45d724448a735735a95924605_615246363_1556413255931774_2968156490140092165_n.jpg";

    public static async Task SeedStudentEnrollmentsAndCertificatesAsync(LearningDbContext context)
    {
        if (await context.Certificates.AnyAsync(c => c.Id == SeedCertificate1Id))
            return;

        var now = DateTime.UtcNow;
        var completedAt = now.AddDays(-7);
        var enrolledAt = now.AddDays(-30);

        // 1 khóa free (đã học xong): ASP.NET Core - enrollment + certificate + progress
        var enrollment1 = new Enrollment
        {
            Id = SeedEnrollment1Id,
            UserId = SeedStudentId,
            CourseId = SeedCourseId,
            CourseTitle = "Lập trình ASP.NET Core từ cơ bản đến nâng cao",
            CourseThumbnailUrl = SeedImageUrl,
            Slug = "lap-trinh-aspnet-core-tu-co-ban-den-nang-cao",
            InstructorId = SeedInstructorId,
            InstructorName = InstructorName,
            PricePaid = 0,
            Status = EnrollmentStatus.Completed,
            ProgressPercent = 100,
            CompletedLessons = 10,
            TotalLessons = 10,
            EnrolledAt = enrolledAt,
            CompletedAt = completedAt,
            CertificateIssuedAt = completedAt,
            CertificateId = SeedCertificate1Id,
            CreatedAt = enrolledAt,
            CreatedBy = Guid.Empty
        };

        var certificate1 = new Certificate
        {
            Id = SeedCertificate1Id,
            EnrollmentId = SeedEnrollment1Id,
            UserId = SeedStudentId,
            CourseId = SeedCourseId,
            CertificateNumber = "CERT-SEED-ASPNET-001",
            StudentName = StudentFullName,
            CourseTitle = enrollment1.CourseTitle,
            InstructorName = InstructorName,
            CompletionDate = completedAt,
            IssuedDate = completedAt,
            VerificationHash = "seed-verify-aspnet-001",
            IsValid = true,
            CreatedAt = completedAt,
            CreatedBy = Guid.Empty
        };

        await context.Enrollments.AddAsync(enrollment1);
        await context.Certificates.AddAsync(certificate1);

        var progressCreatedAt = completedAt.AddHours(-1);

        var sectionProgress1 = new[]
        {
            CreateSectionProgress(SeedStudentId, Section1Id, SeedCourseId, SeedEnrollment1Id, progressCreatedAt),
            CreateSectionProgress(SeedStudentId, Section2Id, SeedCourseId, SeedEnrollment1Id, progressCreatedAt),
            CreateSectionProgress(SeedStudentId, Section3Id, SeedCourseId, SeedEnrollment1Id, progressCreatedAt)
        };
        var lessonProgress1 = new[]
        {
            CreateLessonProgress(SeedStudentId, Lesson1_1Id, SeedCourseId, SeedEnrollment1Id, 900, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson1_2Id, SeedCourseId, SeedEnrollment1Id, 0, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson1_3Id, SeedCourseId, SeedEnrollment1Id, 1800, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson2_1Id, SeedCourseId, SeedEnrollment1Id, 1200, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson2_2Id, SeedCourseId, SeedEnrollment1Id, 0, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson2_3Id, SeedCourseId, SeedEnrollment1Id, 900, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson2_4Id, SeedCourseId, SeedEnrollment1Id, 0, completedAt, isQuiz: true),
            CreateLessonProgress(SeedStudentId, Lesson3_1Id, SeedCourseId, SeedEnrollment1Id, 1500, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson3_2Id, SeedCourseId, SeedEnrollment1Id, 0, completedAt),
            CreateLessonProgress(SeedStudentId, Lesson3_3Id, SeedCourseId, SeedEnrollment1Id, 0, completedAt, isQuiz: true)
        };

        await context.SectionProgresses.AddRangeAsync(sectionProgress1);
        await context.LessonProgresses.AddRangeAsync(lessonProgress1);
        await context.SaveChangesAsync();
    }

    private static SectionProgress CreateSectionProgress(Guid userId, Guid sectionId, Guid courseId, Guid enrollmentId, DateTime createdAt)
    {
        return new SectionProgress
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            SectionId = sectionId,
            CourseId = courseId,
            EnrollmentId = enrollmentId,
            AssignmentSubmitted = true,
            AssignmentGrade = 85m,
            AssignmentSubmittedAt = createdAt,
            AssignmentGradedAt = createdAt,
            CreatedAt = createdAt,
            CreatedBy = Guid.Empty
        };
    }

    private static LessonProgress CreateLessonProgress(Guid userId, Guid lessonId, Guid courseId, Guid enrollmentId, int totalDurationSeconds, DateTime completedAt, bool isQuiz = false)
    {
        return new LessonProgress
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            LessonId = lessonId,
            CourseId = courseId,
            EnrollmentId = enrollmentId,
            Status = LessonProgressStatus.Completed,
            LastPositionSeconds = totalDurationSeconds,
            TotalDurationSeconds = totalDurationSeconds,
            WatchPercent = 100,
            QuizAttempts = isQuiz ? 1 : null,
            QuizBestScore = isQuiz ? 85m : null,
            StartedAt = completedAt.AddHours(-2),
            CompletedAt = completedAt,
            LastAccessedAt = completedAt,
            IsManuallyCompleted = false,
            CreatedAt = completedAt.AddHours(-2),
            CreatedBy = Guid.Empty
        };
    }
}
