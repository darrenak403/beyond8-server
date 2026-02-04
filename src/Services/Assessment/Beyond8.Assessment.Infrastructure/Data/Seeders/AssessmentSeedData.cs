using System.Text.Json;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Assessment.Infrastructure.Data.Seeders;

/// <summary>
/// Seed data cho Assessment service, tương ứng với Catalog seed (course ASP.NET Core).
/// Quiz và Lesson ID phải khớp với CatalogSeedData.
/// </summary>
public static class AssessmentSeedData
{
    // Khớp với Catalog seed
    private static readonly Guid SeedCourseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedInstructorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Lesson2_4Id = Guid.Parse("55555555-5555-5555-5555-555555550204"); // Quiz Section 2
    private static readonly Guid Lesson3_3Id = Guid.Parse("55555555-5555-5555-5555-555555550303"); // Quiz Section 3

    // Quiz IDs (Catalog đã tham chiếu)
    private static readonly Guid Quiz1Id = Guid.Parse("66666666-6666-6666-6666-666666666601");
    private static readonly Guid Quiz2Id = Guid.Parse("66666666-6666-6666-6666-666666666602");

    // Question IDs - Quiz 1 (Section 2: DI, Middleware, Configuration)
    private static readonly Guid Q1_1Id = Guid.Parse("77777777-7777-7777-7777-777777777701");
    private static readonly Guid Q1_2Id = Guid.Parse("77777777-7777-7777-7777-777777777702");
    private static readonly Guid Q1_3Id = Guid.Parse("77777777-7777-7777-7777-777777777703");
    private static readonly Guid Q1_4Id = Guid.Parse("77777777-7777-7777-7777-777777777704");
    private static readonly Guid Q1_5Id = Guid.Parse("77777777-7777-7777-7777-777777777705");

    // Question IDs - Quiz 2 (Section 3: EF Core)
    private static readonly Guid Q2_1Id = Guid.Parse("77777777-7777-7777-7777-777777777706");
    private static readonly Guid Q2_2Id = Guid.Parse("77777777-7777-7777-7777-777777777707");
    private static readonly Guid Q2_3Id = Guid.Parse("77777777-7777-7777-7777-777777777708");
    private static readonly Guid Q2_4Id = Guid.Parse("77777777-7777-7777-7777-777777777709");
    private static readonly Guid Q2_5Id = Guid.Parse("77777777-7777-7777-7777-777777777710");

    // QuizQuestion IDs
    private static readonly Guid Qq1_1Id = Guid.Parse("88888888-8888-8888-8888-888888888801");
    private static readonly Guid Qq1_2Id = Guid.Parse("88888888-8888-8888-8888-888888888802");
    private static readonly Guid Qq1_3Id = Guid.Parse("88888888-8888-8888-8888-888888888803");
    private static readonly Guid Qq1_4Id = Guid.Parse("88888888-8888-8888-8888-888888888804");
    private static readonly Guid Qq1_5Id = Guid.Parse("88888888-8888-8888-8888-888888888805");
    private static readonly Guid Qq2_1Id = Guid.Parse("88888888-8888-8888-8888-888888888806");
    private static readonly Guid Qq2_2Id = Guid.Parse("88888888-8888-8888-8888-888888888807");
    private static readonly Guid Qq2_3Id = Guid.Parse("88888888-8888-8888-8888-888888888808");
    private static readonly Guid Qq2_4Id = Guid.Parse("88888888-8888-8888-8888-888888888809");
    private static readonly Guid Qq2_5Id = Guid.Parse("88888888-8888-8888-8888-888888888810");

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task SeedQuizzesAndQuestionsAsync(AssessmentDbContext context)
    {
        if (await context.Quizzes.AnyAsync(q => q.Id == Quiz1Id || q.Id == Quiz2Id))
            return;

        var now = DateTime.UtcNow;

        // ============ Quiz 1: Kiểm tra kiến thức Section 2 (DI, Middleware, Configuration) ============
        var quiz1 = new Quiz
        {
            Id = Quiz1Id,
            InstructorId = SeedInstructorId,
            CourseId = SeedCourseId,
            LessonId = Lesson2_4Id,
            Title = "Quiz: Kiểm tra kiến thức Section 2",
            Description = "Kiểm tra hiểu biết về Dependency Injection, Middleware và Configuration trong ASP.NET Core",
            TimeLimitMinutes = 15,
            PassScorePercent = 60,
            TotalPoints = 100,
            MaxAttempts = 3,
            ShuffleQuestions = true,
            AllowReview = true,
            ShowExplanation = true,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        };

        // ============ Quiz 2: Entity Framework Core ============
        var quiz2 = new Quiz
        {
            Id = Quiz2Id,
            InstructorId = SeedInstructorId,
            CourseId = SeedCourseId,
            LessonId = Lesson3_3Id,
            Title = "Quiz: Entity Framework Core",
            Description = "Kiểm tra kiến thức về EF Core, DbContext và Migrations",
            TimeLimitMinutes = 15,
            PassScorePercent = 60,
            TotalPoints = 100,
            MaxAttempts = 3,
            ShuffleQuestions = true,
            AllowReview = true,
            ShowExplanation = true,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        };

        await context.Quizzes.AddRangeAsync(quiz1, quiz2);

        // ============ Questions cho Quiz 1 (Section 2) ============
        var questionsQuiz1 = new List<Question>
        {
            CreateQuestion(Q1_1Id, SeedInstructorId, now,
                "Service lifetime nào trong DI Container của ASP.NET Core tạo một instance mới mỗi lần request?",
                [("a", "Singleton", false), ("b", "Scoped", true), ("c", "Transient", false), ("d", "PerRequest", false)],
                "Scoped tạo một instance per request (scope).",
                DifficultyLevel.Medium, ["di", "aspnet"]),
            CreateQuestion(Q1_2Id, SeedInstructorId, now,
                "Middleware trong ASP.NET Core được thực thi theo thứ tự nào?",
                [("a", "Theo thứ tự đăng ký trong pipeline", true), ("b", "Song song", false), ("c", "Ngẫu nhiên", false), ("d", "Theo độ ưu tiên", false)],
                "Middleware chạy tuần tự theo thứ tự Use* khi xử lý request.",
                DifficultyLevel.Easy, ["middleware", "aspnet"]),
            CreateQuestion(Q1_3Id, SeedInstructorId, now,
                "Options Pattern trong ASP.NET Core thường dùng với interface nào?",
                [("a", "IOptions<T>", false), ("b", "IOptionsSnapshot<T>", false), ("c", "Cả IOptions<T> và IOptionsSnapshot<T>", true), ("d", "IConfiguration only", false)],
                "IOptions cho singleton; IOptionsSnapshot cho per-request (reload khi config thay đổi).",
                DifficultyLevel.Medium, ["configuration", "options"]),
            CreateQuestion(Q1_4Id, SeedInstructorId, now,
                "RequestDelegate trong Middleware đại diện cho gì?",
                [("a", "HTTP request object", false), ("b", "Middleware tiếp theo trong pipeline", true), ("c", "Response stream", false), ("d", "Endpoint handler", false)],
                "RequestDelegate là delegate gọi middleware tiếp theo.",
                DifficultyLevel.Medium, ["middleware"]),
            CreateQuestion(Q1_5Id, SeedInstructorId, now,
                "Phương thức nào dùng để đăng ký service với lifetime Transient?",
                [("a", "AddSingleton", false), ("b", "AddScoped", false), ("c", "AddTransient", true), ("d", "AddInstance", false)],
                "AddTransient tạo instance mới mỗi lần resolve.",
                DifficultyLevel.Easy, ["di"]),
        };

        // ============ Questions cho Quiz 2 (Section 3: EF Core) ============
        var questionsQuiz2 = new List<Question>
        {
            CreateQuestion(Q2_1Id, SeedInstructorId, now,
                "DbContext trong EF Core đại diện cho gì?",
                [("a", "Một bảng trong database", false), ("b", "Session làm việc với database, bao gồm nhiều entity", true), ("c", "Connection string", false), ("d", "Migration file", false)],
                "DbContext là unit of work và repository cho các entity được map.",
                DifficultyLevel.Easy, ["efcore", "dbcontext"]),
            CreateQuestion(Q2_2Id, SeedInstructorId, now,
                "Lệnh nào tạo migration mới trong EF Core?",
                [("a", "dotnet ef database update", false), ("b", "dotnet ef migrations add <TênMigration>", true), ("c", "dotnet ef migrations remove", false), ("d", "dotnet ef migrations list", false)],
                "dotnet ef migrations add tạo migration từ thay đổi model.",
                DifficultyLevel.Easy, ["migrations", "efcore"]),
            CreateQuestion(Q2_3Id, SeedInstructorId, now,
                "Sau khi sửa migration đã apply lên production, nên làm gì?",
                [("a", "Sửa file migration và chạy lại update", false), ("b", "Tạo migration mới để thay đổi schema", true), ("c", "Xóa database và tạo lại", false), ("d", "Rollback rồi sửa migration", false)],
                "Best practice: không sửa migration đã apply, tạo migration mới.",
                DifficultyLevel.Medium, ["migrations", "best-practices"]),
            CreateQuestion(Q2_4Id, SeedInstructorId, now,
                "OnModelCreating trong DbContext dùng để làm gì?",
                [("a", "Kết nối database", false), ("b", "Cấu hình entity, relationship, index", true), ("c", "Chạy raw SQL", false), ("d", "Seed data", false)],
                "Fluent API cấu hình model trong OnModelCreating.",
                DifficultyLevel.Medium, ["efcore", "dbcontext"]),
            CreateQuestion(Q2_5Id, SeedInstructorId, now,
                "Lệnh dotnet ef migrations script dùng để làm gì?",
                [("a", "Apply migration", false), ("b", "Sinh ra file SQL từ migrations", true), ("c", "Xóa migration", false), ("d", "Backup database", false)],
                "Script sinh SQL để review hoặc chạy thủ công.",
                DifficultyLevel.Easy, ["migrations"]),
        };

        await context.Questions.AddRangeAsync(questionsQuiz1);
        await context.Questions.AddRangeAsync(questionsQuiz2);

        // ============ QuizQuestion: gắn câu hỏi vào quiz ============
        var quizQuestions = new List<QuizQuestion>
        {
            CreateQuizQuestion(Qq1_1Id, Quiz1Id, Q1_1Id, 1, now, SeedInstructorId),
            CreateQuizQuestion(Qq1_2Id, Quiz1Id, Q1_2Id, 2, now, SeedInstructorId),
            CreateQuizQuestion(Qq1_3Id, Quiz1Id, Q1_3Id, 3, now, SeedInstructorId),
            CreateQuizQuestion(Qq1_4Id, Quiz1Id, Q1_4Id, 4, now, SeedInstructorId),
            CreateQuizQuestion(Qq1_5Id, Quiz1Id, Q1_5Id, 5, now, SeedInstructorId),
            CreateQuizQuestion(Qq2_1Id, Quiz2Id, Q2_1Id, 1, now, SeedInstructorId),
            CreateQuizQuestion(Qq2_2Id, Quiz2Id, Q2_2Id, 2, now, SeedInstructorId),
            CreateQuizQuestion(Qq2_3Id, Quiz2Id, Q2_3Id, 3, now, SeedInstructorId),
            CreateQuizQuestion(Qq2_4Id, Quiz2Id, Q2_4Id, 4, now, SeedInstructorId),
            CreateQuizQuestion(Qq2_5Id, Quiz2Id, Q2_5Id, 5, now, SeedInstructorId),
        };

        await context.QuizQuestions.AddRangeAsync(quizQuestions);

        await context.SaveChangesAsync();
    }

    private static Question CreateQuestion(
        Guid id,
        Guid instructorId,
        DateTime now,
        string content,
        (string id, string text, bool isCorrect)[] options,
        string explanation,
        DifficultyLevel difficulty,
        string[] tags)
    {
        var optionList = options
            .Select(o => new QuestionOptionItem { Id = o.id, Text = o.text, IsCorrect = o.isCorrect })
            .ToList();
        return new Question
        {
            Id = id,
            InstructorId = instructorId,
            Content = content,
            Type = QuestionType.MultipleChoice,
            Options = JsonSerializer.Serialize(optionList, JsonOptions),
            Explanation = explanation,
            Difficulty = difficulty,
            Points = 20m,
            Tags = JsonSerializer.Serialize(tags, JsonOptions),
            IsActive = true,
            CreatedAt = now,
            CreatedBy = instructorId
        };
    }

    private static QuizQuestion CreateQuizQuestion(Guid id, Guid quizId, Guid questionId, int orderIndex, DateTime now, Guid createdBy)
    {
        return new QuizQuestion
        {
            Id = id,
            QuizId = quizId,
            QuestionId = questionId,
            OrderIndex = orderIndex,
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }
}
