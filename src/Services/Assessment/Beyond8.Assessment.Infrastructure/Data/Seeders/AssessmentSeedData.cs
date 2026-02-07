using System.Text.Json;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Assessment.Infrastructure.Data.Seeders;

public static class AssessmentSeedData
{
    // Course 1: Free (ASP.NET Core)
    private static readonly Guid SeedCourseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Lesson2_4Id = Guid.Parse("55555555-5555-5555-5555-555555550204"); // Section 2 - Quiz lesson
    private static readonly Guid Lesson3_3Id = Guid.Parse("55555555-5555-5555-5555-555555550303"); // Section 3 - Quiz lesson

    // Course 2: Paid (Microservices)
    private static readonly Guid PaidCourseId = Guid.Parse("33333333-3333-3333-3333-333333333334");
    private static readonly Guid PaidLesson3Id = Guid.Parse("55555555-5555-5555-5555-555555550403"); // Paid section - Quiz lesson

    private static readonly Guid SeedInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000006"); // Trần Thị Giảng Viên (Identity)

    // Section IDs (Catalog sections - cùng GUID với CatalogSeedData)
    private static readonly Guid Section1Id = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private static readonly Guid Section2Id = Guid.Parse("44444444-4444-4444-4444-444444444402");
    private static readonly Guid Section3Id = Guid.Parse("44444444-4444-4444-4444-444444444403");
    private static readonly Guid PaidSectionId = Guid.Parse("44444444-4444-4444-4444-444444444404");

    // Quiz IDs (Catalog LessonQuiz tham chiếu tới)
    private static readonly Guid Quiz1Id = Guid.Parse("66666666-6666-6666-6666-666666666601");
    private static readonly Guid Quiz2Id = Guid.Parse("66666666-6666-6666-6666-666666666602");
    private static readonly Guid Quiz3Id = Guid.Parse("66666666-6666-6666-6666-666666666603");

    // Assignment IDs (Catalog Section.AssignmentId tham chiếu tới)
    private static readonly Guid Assignment1Id = Guid.Parse("66666666-6666-6666-6666-666666666701");
    private static readonly Guid Assignment2Id = Guid.Parse("66666666-6666-6666-6666-666666666702");
    private static readonly Guid Assignment3Id = Guid.Parse("66666666-6666-6666-6666-666666666703");
    private static readonly Guid Assignment4Id = Guid.Parse("66666666-6666-6666-6666-666666666704");

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

    // Question IDs - Quiz 3 (Paid course: Microservices & Docker)
    private static readonly Guid Q3_1Id = Guid.Parse("77777777-7777-7777-7777-777777777711");
    private static readonly Guid Q3_2Id = Guid.Parse("77777777-7777-7777-7777-777777777712");
    private static readonly Guid Q3_3Id = Guid.Parse("77777777-7777-7777-7777-777777777713");
    private static readonly Guid Q3_4Id = Guid.Parse("77777777-7777-7777-7777-777777777714");

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
    private static readonly Guid Qq3_1Id = Guid.Parse("88888888-8888-8888-8888-888888888811");
    private static readonly Guid Qq3_2Id = Guid.Parse("88888888-8888-8888-8888-888888888812");
    private static readonly Guid Qq3_3Id = Guid.Parse("88888888-8888-8888-8888-888888888813");
    private static readonly Guid Qq3_4Id = Guid.Parse("88888888-8888-8888-8888-888888888814");

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static string SerializeAttachmentUrls(List<(string Name, string Url)> items) =>
        JsonSerializer.Serialize(items.Select(x => new AssignmentAttachmentItem { Name = x.Name, Url = x.Url }).ToList(), JsonOptions);

    public static async Task SeedQuizzesAndQuestionsAsync(AssessmentDbContext context)
    {
        var hasQuiz3 = await context.Quizzes.AnyAsync(q => q.Id == Quiz3Id);
        if (hasQuiz3)
            return; // Đã seed đủ cả 2 course

        var now = DateTime.UtcNow;
        var hasQuiz1 = await context.Quizzes.AnyAsync(q => q.Id == Quiz1Id);

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

        // ============ Quiz 2: Entity Framework Core (Course 1) ============
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

        // ============ Quiz 3: Microservices & Docker (Course 2 - Paid) ============
        var quiz3 = new Quiz
        {
            Id = Quiz3Id,
            InstructorId = SeedInstructorId,
            CourseId = PaidCourseId,
            LessonId = PaidLesson3Id,
            Title = "Quiz: Kiểm tra Microservices & Docker",
            Description = "Kiểm tra kiến thức về Microservices và Docker cơ bản",
            TimeLimitMinutes = 10,
            PassScorePercent = 60,
            TotalPoints = 100,
            MaxAttempts = 2,
            ShuffleQuestions = true,
            AllowReview = true,
            ShowExplanation = true,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = SeedInstructorId
        };

        if (!hasQuiz1)
        {
            await context.Quizzes.AddRangeAsync(quiz1, quiz2, quiz3);
        }
        else
        {
            await context.Quizzes.AddAsync(quiz3);
        }

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

        // ============ Questions cho Quiz 3 (Paid course: Microservices & Docker) ============
        var questionsQuiz3 = new List<Question>
        {
            CreateQuestion(Q3_1Id, SeedInstructorId, now,
                "Kiến trúc Microservices khác Monolith chủ yếu ở điểm nào?",
                [("a", "Chạy trên một server duy nhất", false), ("b", "Ứng dụng được tách thành nhiều service độc lập, deploy và scale riêng", true), ("c", "Dùng một database chung", false), ("d", "Chỉ dùng .NET", false)],
                "Microservices tách ứng dụng thành các service nhỏ, độc lập.",
                DifficultyLevel.Easy, ["microservices"]),
            CreateQuestion(Q3_2Id, SeedInstructorId, now,
                "Docker Container khác VM (máy ảo) như thế nào?",
                [("a", "Container có OS riêng, VM dùng chung kernel", false), ("b", "Container dùng chung kernel host, nhẹ và khởi động nhanh hơn VM", true), ("c", "VM nhẹ hơn Container", false), ("d", "Không có khác biệt", false)],
                "Container chia sẻ kernel của host, không cần OS đầy đủ cho mỗi instance.",
                DifficultyLevel.Medium, ["docker", "container"]),
            CreateQuestion(Q3_3Id, SeedInstructorId, now,
                "Dockerfile dùng để làm gì?",
                [("a", "Chạy container", false), ("b", "Định nghĩa cách build Docker image", true), ("c", "Quản lý nhiều container", false), ("d", "Cấu hình network", false)],
                "Dockerfile chứa các lệnh để build image (FROM, COPY, RUN, CMD...).",
                DifficultyLevel.Easy, ["docker", "dockerfile"]),
            CreateQuestion(Q3_4Id, SeedInstructorId, now,
                "Giao tiếp giữa các Microservice thường dùng cơ chế nào?",
                [("a", "Chỉ gọi trực tiếp hàm trong process", false), ("b", "HTTP/REST, gRPC, hoặc message queue (RabbitMQ, Kafka)", true), ("c", "Chỉ shared database", false), ("d", "Chỉ file system chung", false)],
                "Các service giao tiếp qua network: REST API, gRPC, hoặc messaging.",
                DifficultyLevel.Medium, ["microservices", "messaging"]),
        };

        if (!hasQuiz1)
        {
            await context.Questions.AddRangeAsync(questionsQuiz1);
            await context.Questions.AddRangeAsync(questionsQuiz2);
        }
        await context.Questions.AddRangeAsync(questionsQuiz3);

        // ============ QuizQuestion: gắn câu hỏi vào quiz ============
        var quizQuestions = new List<QuizQuestion>
        {
            CreateQuizQuestion(Qq3_1Id, Quiz3Id, Q3_1Id, 1, now, SeedInstructorId),
            CreateQuizQuestion(Qq3_2Id, Quiz3Id, Q3_2Id, 2, now, SeedInstructorId),
            CreateQuizQuestion(Qq3_3Id, Quiz3Id, Q3_3Id, 3, now, SeedInstructorId),
            CreateQuizQuestion(Qq3_4Id, Quiz3Id, Q3_4Id, 4, now, SeedInstructorId),
        };
        if (!hasQuiz1)
        {
            quizQuestions.InsertRange(0, new[]
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
            });
        }

        await context.QuizQuestions.AddRangeAsync(quizQuestions);

        await context.SaveChangesAsync();
    }

    public static async Task SeedAssignmentsAsync(AssessmentDbContext context)
    {
        if (await context.Assignments.AnyAsync(a => a.Id == Assignment1Id))
            return;

        var now = DateTime.UtcNow;
        var assignments = new List<Assignment>
        {
            new()
            {
                Id = Assignment1Id,
                InstructorId = SeedInstructorId,
                CourseId = SeedCourseId,
                SectionId = Section1Id,
                Title = "Bài tập: Giới thiệu ASP.NET Core",
                Description = "Demo - Làm lần lượt:\n1. Cài đặt .NET SDK và kiểm tra phiên bản.\n2. Tạo project ASP.NET Core Web API mới.\n3. Chạy ứng dụng và gọi endpoint mặc định.\n4. Nộp file zip project hoặc link repository.",
                AttachmentUrls = SerializeAttachmentUrls(
                [
                    ("Tài liệu .NET SDK", "https://learn.microsoft.com/dotnet/core/install"),
                    ("ASP.NET Core docs", "https://learn.microsoft.com/aspnet/core")
                ]),
                RubricUrl = "https://d30z0qh7rhzgt8.cloudfront.net/courses/rubrics/Rubric-Percent-Assignment-1-Aspnet-Core.pdf",
                SubmissionType = AssignmentSubmissionType.File,
                GradingMode = GradingMode.AiAssisted,
                TotalPoints = 100,
                TimeLimitMinutes = 60,
                CreatedAt = now,
                CreatedBy = SeedInstructorId
            },
            new()
            {
                Id = Assignment2Id,
                InstructorId = SeedInstructorId,
                CourseId = SeedCourseId,
                SectionId = Section2Id,
                Title = "Bài tập: Dependency Injection và Middleware",
                Description = "Demo - Làm lần lượt:\n1. Đăng ký một service với các lifetime Singleton, Scoped, Transient.\n2. Inject và sử dụng service trong controller.\n3. Viết middleware in ra thời gian xử lý request.\n4. Đăng ký middleware vào pipeline và kiểm tra.\n5. Nộp mã nguồn hoặc file mô tả.",
                AttachmentUrls = SerializeAttachmentUrls(
                [
                    ("Dependency Injection", "https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection"),
                    ("Middleware", "https://learn.microsoft.com/aspnet/core/fundamentals/middleware")
                ]),
                RubricUrl = "https://d30z0qh7rhzgt8.cloudfront.net/courses/rubrics/Rubric-Percent-Assignment-2-Di-Middleware.pdf",
                SubmissionType = AssignmentSubmissionType.Both,
                GradingMode = GradingMode.AiAssisted,
                TotalPoints = 100,
                TimeLimitMinutes = 90,
                CreatedAt = now,
                CreatedBy = SeedInstructorId
            },
            new()
            {
                Id = Assignment3Id,
                InstructorId = SeedInstructorId,
                CourseId = SeedCourseId,
                SectionId = Section3Id,
                Title = "Bài tập: Entity Framework Core",
                Description = "Demo - Làm lần lượt:\n1. Tạo entity class và DbContext.\n2. Cấu hình connection string và đăng ký DbContext.\n3. Tạo migration đầu tiên và cập nhật database.\n4. Thêm seed data (nếu cần) và migration thứ hai.\n5. Viết truy vấn LINQ: Where, OrderBy, Include.\n6. Nộp file zip project hoặc link repository.",
                AttachmentUrls = SerializeAttachmentUrls(
                [
                    ("EF Core Getting Started", "https://learn.microsoft.com/ef/core/get-started/overview/first-app"),
                    ("Migrations", "https://learn.microsoft.com/ef/core/managing-schemas/migrations"),
                    ("LINQ queries", "https://learn.microsoft.com/ef/core/querying/")
                ]),
                RubricUrl = "https://d30z0qh7rhzgt8.cloudfront.net/courses/rubrics/Rubric-Percent-Assignment-3-Ef-Core.pdf",
                SubmissionType = AssignmentSubmissionType.File,
                GradingMode = GradingMode.AiAssisted,
                TotalPoints = 100,
                TimeLimitMinutes = 120,
                CreatedAt = now,
                CreatedBy = SeedInstructorId
            },
            new()
            {
                Id = Assignment4Id,
                InstructorId = SeedInstructorId,
                CourseId = PaidCourseId,
                SectionId = PaidSectionId,
                Title = "Bài tập: Microservices và Docker",
                Description = "Demo - Làm lần lượt:\n1. Vẽ sơ đồ kiến trúc microservices (3–5 service) và mô tả chức năng từng service.\n2. Mô tả cách giao tiếp giữa các service (REST/gRPC/message queue).\n3. Viết Dockerfile cho một API .NET mẫu.\n4. Build image và chạy container, chụp màn hình hoặc ghi lại lệnh.\n5. Nộp file mô tả (PDF/Word) kèm Dockerfile và ảnh.",
                AttachmentUrls = SerializeAttachmentUrls(
                [
                    ("Microservices với .NET", "https://learn.microsoft.com/dotnet/architecture/microservices"),
                    ("Docker docs", "https://docs.docker.com/get-started/"),
                    ("Dockerfile reference", "https://docs.docker.com/engine/reference/builder/")
                ]),
                RubricUrl = "https://d30z0qh7rhzgt8.cloudfront.net/courses/rubrics/Rubric-Percent-Assignment-4-Microservices-Docker.pdf",
                SubmissionType = AssignmentSubmissionType.Both,
                GradingMode = GradingMode.AiAssisted,
                TotalPoints = 100,
                TimeLimitMinutes = 90,
                CreatedAt = now,
                CreatedBy = SeedInstructorId
            }
        };

        await context.Assignments.AddRangeAsync(assignments);
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
