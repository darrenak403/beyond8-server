using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data.Seeders;

public static class CatalogSeedData
{
    // Fixed GUIDs for consistent seeding (instructor trùng Identity seed: instructor@gmail.com)
    private static readonly Guid SeedCourseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid PaidCourseId = Guid.Parse("33333333-3333-3333-3333-333333333334");
    private static readonly Guid SeedInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000006"); // Trần Thị Giảng Viên (Identity)

    // Section IDs
    private static readonly Guid Section1Id = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private static readonly Guid Section2Id = Guid.Parse("44444444-4444-4444-4444-444444444402");
    private static readonly Guid Section3Id = Guid.Parse("44444444-4444-4444-4444-444444444403");

    // Lesson IDs - Section 1 (Introduction)
    private static readonly Guid Lesson1_1Id = Guid.Parse("55555555-5555-5555-5555-555555550101");
    private static readonly Guid Lesson1_2Id = Guid.Parse("55555555-5555-5555-5555-555555550102");
    private static readonly Guid Lesson1_3Id = Guid.Parse("55555555-5555-5555-5555-555555550103");

    // Lesson IDs - Section 2 (Core Concepts)
    private static readonly Guid Lesson2_1Id = Guid.Parse("55555555-5555-5555-5555-555555550201");
    private static readonly Guid Lesson2_2Id = Guid.Parse("55555555-5555-5555-5555-555555550202");
    private static readonly Guid Lesson2_3Id = Guid.Parse("55555555-5555-5555-5555-555555550203");
    private static readonly Guid Lesson2_4Id = Guid.Parse("55555555-5555-5555-5555-555555550204");

    // Lesson IDs - Section 3 (Advanced Topics)
    private static readonly Guid Lesson3_1Id = Guid.Parse("55555555-5555-5555-5555-555555550301");
    private static readonly Guid Lesson3_2Id = Guid.Parse("55555555-5555-5555-5555-555555550302");
    private static readonly Guid Lesson3_3Id = Guid.Parse("55555555-5555-5555-5555-555555550303");

    // Quiz IDs (external - from Assessment Service)
    private static readonly Guid Quiz1Id = Guid.Parse("66666666-6666-6666-6666-666666666601");
    private static readonly Guid Quiz2Id = Guid.Parse("66666666-6666-6666-6666-666666666602");
    private static readonly Guid Quiz3Id = Guid.Parse("66666666-6666-6666-6666-666666666603");

    // Assignment IDs (external - from Assessment Service)
    private static readonly Guid Assignment1Id = Guid.Parse("66666666-6666-6666-6666-666666666701");
    private static readonly Guid Assignment2Id = Guid.Parse("66666666-6666-6666-6666-666666666702");
    private static readonly Guid Assignment3Id = Guid.Parse("66666666-6666-6666-6666-666666666703");
    private static readonly Guid Assignment4Id = Guid.Parse("66666666-6666-6666-6666-666666666704");

    // Paid course: 1 section, 3 lessons (Video, Text, Quiz)
    private static readonly Guid PaidSectionId = Guid.Parse("44444444-4444-4444-4444-444444444404");
    private static readonly Guid PaidLesson1Id = Guid.Parse("55555555-5555-5555-5555-555555550401");
    private static readonly Guid PaidLesson2Id = Guid.Parse("55555555-5555-5555-5555-555555550402");
    private static readonly Guid PaidLesson3Id = Guid.Parse("55555555-5555-5555-5555-555555550403");

    // Seed media URLs (CloudFront)
    private const string SeedVideoUrl = "https://d30z0qh7rhzgt8.cloudfront.net/courses/hls/meo_con_lon_ton/meo_con_lon_ton_1080p.m3u8";
    private const string SeedImageUrl = "https://d30z0qh7rhzgt8.cloudfront.net/course/thumbnails/00000000-0000-0000-0000-000000000006/7657afd45d724448a735735a95924605_615246363_1556413255931774_2968156490140092165_n.jpg";

    public static async Task SeedCategoriesAsync(CatalogDbContext context)
    {
        // Kiểm tra xem đã có dữ liệu chưa
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>();

        // --- 1. LẬP TRÌNH ---
        var devRoot = CreateCategory("Lập trình", "lap-trinh", "Các khóa học về tư duy và kỹ thuật phần mềm.", CategoryType.Technology);
        categories.Add(devRoot);
        categories.AddRange(CreateSubCategories(devRoot,
        [
            ("Lập trình Web", "lap-trinh-web", "Frontend, Backend, Fullstack."),
            ("Lập trình Mobile", "lap-trinh-mobile", "iOS, Android, React Native, Flutter.")
        ]));

        // --- 2. THIẾT KẾ ---
        var designRoot = CreateCategory("Thiết kế", "thiet-ke", "Tư duy thẩm mỹ và công cụ sáng tạo.", CategoryType.Design);
        categories.Add(designRoot);
        categories.AddRange(CreateSubCategories(designRoot,
        [
            ("Thiết kế UI/UX", "thiet-ke-ui-ux", "Trải nghiệm và giao diện người dùng."),
            ("Thiết kế Đồ họa", "thiet-ke-do-hoa", "Photoshop, AI, Branding.")
        ]));

        // --- 3. NGÔN NGỮ ---
        var langRoot = CreateCategory("Ngôn ngữ", "ngon-ngu", "Đào tạo ngoại ngữ các cấp độ.", CategoryType.Language);
        categories.Add(langRoot);
        categories.AddRange(CreateSubCategories(langRoot,
        [
            ("Tiếng Anh", "tieng-anh", "Giao tiếp, TOEIC, IELTS."),
            ("Tiếng Nhật", "tieng-nhat", "Sơ cấp N5 đến Cao cấp N1.")
        ]));

        // --- 4. KINH DOANH ---
        var bizRoot = CreateCategory("Kinh doanh", "kinh-doanh", "Kiến thức quản trị và khởi nghiệp.", CategoryType.Business);
        categories.Add(bizRoot);
        categories.AddRange(CreateSubCategories(bizRoot,
        [
            ("Khởi nghiệp", "khoi-nghiep", "Xây dựng mô hình kinh doanh."),
            ("Quản trị nhân sự", "quan-tri-nhan-su", "Tuyển dụng và đào tạo nhân tài.")
        ]));

        // --- 5. MARKETING ---
        var mktRoot = CreateCategory("Marketing", "marketing", "Tiếp thị số và thương hiệu.", CategoryType.Marketing);
        categories.Add(mktRoot);
        categories.AddRange(CreateSubCategories(mktRoot,
        [
            ("Digital Marketing", "digital-marketing", "SEO, Google Ads, Social Media."),
            ("Content Marketing", "content-marketing", "Copywriting và sáng tạo nội dung.")
        ]));

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static Category CreateCategory(
        string name,
        string slug,
        string description,
        CategoryType type = CategoryType.Other)
    {
        var id = Guid.NewGuid();
        return new Category
        {
            Id = id,
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = null,
            Level = 0,
            Path = id.ToString(),
            IsActive = true,
            TotalCourses = 0,
            IsRoot = true,
            Type = type
        };
    }

    private static IEnumerable<Category> CreateSubCategories(
        Category parent,
        (string Name, string Slug, string Desc)[] subs)
    {
        var list = new List<Category>();
        foreach (var sub in subs)
        {
            var id = Guid.NewGuid();
            list.Add(new Category
            {
                Id = id,
                Name = sub.Name,
                Slug = sub.Slug,
                Description = sub.Desc,
                ParentId = parent.Id,
                Level = parent.Level + 1,
                Path = $"{parent.Path}/{id}",
                IsActive = true,
                TotalCourses = 0,
                IsRoot = false,
                Type = parent.Type
            });
        }
        return list;
    }

    public static async Task SeedCoursesAsync(CatalogDbContext context)
    {
        // Get the "Lập trình Web" category
        var webDevCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "lap-trinh-web");
        if (webDevCategory == null)
        {
            // Fallback: get any technology category
            webDevCategory = await context.Categories.FirstOrDefaultAsync(c => c.Type == CategoryType.Technology);
        }

        if (webDevCategory == null)
        {
            return; // Cannot seed without categories
        }

        // Cập nhật khóa free (giá + discount 100% = miễn phí) nếu đã tồn tại
        var existingFreeCourse = await context.Courses.FirstOrDefaultAsync(c => c.Id == SeedCourseId);
        if (existingFreeCourse != null)
        {
            existingFreeCourse.Price = 500_000;
            existingFreeCourse.DiscountPercent = 100;
            existingFreeCourse.DiscountEndsAt = DateTime.UtcNow.AddMonths(3);
            context.Courses.Update(existingFreeCourse);
            await context.SaveChangesAsync();

            // Thêm khóa có phí nếu chưa có
            if (!await context.Courses.AnyAsync(c => c.Id == PaidCourseId))
            {
                await AddPaidCourseAsync(context, webDevCategory);
            }
            return;
        }

        // 1. Create Course (free: giá 500k, discount 100% = FinalPrice 0)
        var course = new Course
        {
            Id = SeedCourseId,
            InstructorId = SeedInstructorId,
            InstructorName = "Trần Thị Giảng Viên",
            InstructorVerificationStatus = InstructorVerificationStatus.Verified,
            CategoryId = webDevCategory.Id,
            Title = "Lập trình ASP.NET Core từ cơ bản đến nâng cao",
            Slug = "lap-trinh-aspnet-core-tu-co-ban-den-nang-cao",
            Description = @"Khóa học toàn diện về ASP.NET Core, giúp bạn xây dựng ứng dụng web hiện đại với .NET.

Trong khóa học này, bạn sẽ học:
- Nền tảng ASP.NET Core và kiến trúc MVC
- Dependency Injection và Middleware
- Entity Framework Core và cách làm việc với database
- RESTful API với Minimal APIs
- Authentication và Authorization với JWT
- Clean Architecture và best practices
- Deploy ứng dụng lên cloud

Khóa học phù hợp cho cả người mới bắt đầu và những developer muốn nâng cao kỹ năng .NET.",
            ShortDescription = "Học ASP.NET Core từ zero đến hero với các dự án thực tế",
            Price = 500_000,
            DiscountPercent = 100,
            DiscountEndsAt = DateTime.UtcNow.AddMonths(3),
            Status = CourseStatus.Published,
            Level = CourseLevel.Beginner,
            Language = "vi-VN",
            ThumbnailUrl = SeedImageUrl,
            Outcomes = "[\"Xây dựng ứng dụng web hoàn chỉnh với ASP.NET Core\", \"Thiết kế RESTful API theo best practices\", \"Làm việc với Entity Framework Core và PostgreSQL\", \"Triển khai Authentication/Authorization với JWT\", \"Áp dụng Clean Architecture trong dự án thực tế\", \"Deploy ứng dụng lên Azure/AWS\"]",
            Requirements = "[\"Kiến thức cơ bản về C#\", \"Hiểu biết về HTML, CSS, JavaScript\", \"Máy tính cài đặt .NET SDK 8.0 trở lên\"]",
            TargetAudience = "[\"Sinh viên CNTT muốn học lập trình web\", \"Developer muốn chuyển sang .NET\", \"Backend developer muốn nâng cao kỹ năng\"]",
            AvgRating = 4.8m,
            TotalReviews = 320,
            TotalRatings = 450,
            ApprovedBy = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            ApprovedAt = DateTime.UtcNow.AddDays(-30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await context.Courses.AddAsync(course);

        // Update category course count
        webDevCategory.TotalCourses++;

        // 2. Create Sections
        var sections = new List<Section>
        {
            new()
            {
                Id = Section1Id,
                CourseId = SeedCourseId,
                Title = "Giới thiệu ASP.NET Core",
                Description = "Tổng quan về .NET và ASP.NET Core, cài đặt môi trường phát triển",
                OrderIndex = 1,
                IsPublished = true,
                TotalLessons = 3,
                TotalDurationMinutes = 90,
                AssignmentId = Assignment1Id,
                CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new()
            {
                Id = Section2Id,
                CourseId = SeedCourseId,
                Title = "Các khái niệm cốt lõi",
                Description = "Dependency Injection, Middleware, Configuration và Logging",
                OrderIndex = 2,
                IsPublished = true,
                TotalLessons = 4,
                TotalDurationMinutes = 180,
                AssignmentId = Assignment2Id,
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new()
            {
                Id = Section3Id,
                CourseId = SeedCourseId,
                Title = "Entity Framework Core",
                Description = "Làm việc với database sử dụng EF Core, Migrations và LINQ",
                OrderIndex = 3,
                IsPublished = true,
                TotalLessons = 3,
                TotalDurationMinutes = 210,
                AssignmentId = Assignment3Id,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            }
        };

        await context.Sections.AddRangeAsync(sections);

        // 3. Create Lessons with Video, Text, Quiz types
        await SeedLessonsAsync(context);

        // 4. Paid course (có phí)
        await AddPaidCourseAsync(context, webDevCategory);

        await context.SaveChangesAsync();
    }

    private static async Task AddPaidCourseAsync(CatalogDbContext context, Category webDevCategory)
    {
        if (await context.Courses.AnyAsync(c => c.Id == PaidCourseId))
        {
            // Paid course đã tồn tại: đảm bảo có lesson Quiz (Assessment Quiz3) nếu chưa có
            if (!await context.Lessons.AnyAsync(l => l.Id == PaidLesson3Id))
            {
                var lesson3 = new Lesson
                {
                    Id = PaidLesson3Id,
                    SectionId = PaidSectionId,
                    Title = "Quiz: Kiểm tra Microservices & Docker",
                    Description = "Kiểm tra kiến thức về Microservices và Docker cơ bản",
                    Type = LessonType.Quiz,
                    OrderIndex = 3,
                    IsPreview = false,
                    IsPublished = true,
                    TotalViews = 0,
                    TotalCompletions = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                };
                var lessonQuiz3 = new LessonQuiz
                {
                    Id = Guid.NewGuid(),
                    LessonId = PaidLesson3Id,
                    QuizId = Quiz3Id
                };
                await context.Lessons.AddAsync(lesson3);
                await context.LessonQuizzes.AddAsync(lessonQuiz3);
                var section = await context.Sections.FindAsync(PaidSectionId);
                if (section != null)
                {
                    section.TotalLessons = 3;
                    section.TotalDurationMinutes = 60;
                }
                await context.SaveChangesAsync();
            }
            return;
        }

        var paidCourse = new Course
        {
            Id = PaidCourseId,
            InstructorId = SeedInstructorId,
            InstructorName = "Trần Thị Giảng Viên",
            InstructorVerificationStatus = InstructorVerificationStatus.Verified,
            CategoryId = webDevCategory.Id,
            Title = "Microservices với .NET và Docker",
            Slug = "microservices-voi-dotnet-va-docker",
            Description = "Xây dựng kiến trúc microservices với .NET, Docker, message queue. Phù hợp developer đã có kinh nghiệm ASP.NET Core.",
            ShortDescription = "Kiến trúc microservices, container, messaging",
            Price = 299_000,
            DiscountPercent = null,
            DiscountAmount = null,
            DiscountEndsAt = null,
            Status = CourseStatus.Published,
            Level = CourseLevel.Intermediate,
            Language = "vi-VN",
            ThumbnailUrl = SeedImageUrl,
            Outcomes = "[\"Thiết kế và triển khai microservices\", \"Docker và container\", \"Giao tiếp giữa các service\"]",
            Requirements = "[\"Đã học ASP.NET Core\", \"Hiểu REST API\"]",
            TargetAudience = "[\"Backend developer\", \"DevOps\"]",
            AvgRating = null,
            TotalReviews = 0,
            TotalRatings = 0,
            ApprovedBy = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            ApprovedAt = DateTime.UtcNow.AddDays(-14),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };
        await context.Courses.AddAsync(paidCourse);
        webDevCategory.TotalCourses++;

        var paidSection = new Section
        {
            Id = PaidSectionId,
            CourseId = PaidCourseId,
            Title = "Giới thiệu Microservices",
            Description = "Khái niệm và kiến trúc",
            OrderIndex = 1,
            IsPublished = true,
            TotalLessons = 3,
            TotalDurationMinutes = 60,
            AssignmentId = Assignment4Id,
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        };
        await context.Sections.AddAsync(paidSection);

        var paidLesson1 = new Lesson
        {
            Id = PaidLesson1Id,
            SectionId = PaidSectionId,
            Title = "Tổng quan Microservices",
            Description = "So sánh Monolith và Microservices",
            Type = LessonType.Video,
            OrderIndex = 1,
            IsPreview = true,
            IsPublished = true,
            TotalViews = 0,
            TotalCompletions = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-16)
        };
        var paidVideo1 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = PaidLesson1Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 1200,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = false
        };

        var paidLesson2 = new Lesson
        {
            Id = PaidLesson2Id,
            SectionId = PaidSectionId,
            Title = "Docker cơ bản",
            Description = "Container và Dockerfile",
            Type = LessonType.Text,
            OrderIndex = 2,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 0,
            TotalCompletions = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };
        var paidText2 = new LessonText
        {
            Id = Guid.NewGuid(),
            LessonId = PaidLesson2Id,
            TextContent = "# Docker cơ bản\n\n## Container\n\nContainer đóng gói ứng dụng và dependencies..."
        };

        // Lesson 3 - Quiz (Assessment Service: Quiz3)
        var paidLesson3 = new Lesson
        {
            Id = PaidLesson3Id,
            SectionId = PaidSectionId,
            Title = "Quiz: Kiểm tra Microservices & Docker",
            Description = "Kiểm tra kiến thức về Microservices và Docker cơ bản",
            Type = LessonType.Quiz,
            OrderIndex = 3,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 0,
            TotalCompletions = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        };

        var paidQuiz3 = new LessonQuiz
        {
            Id = Guid.NewGuid(),
            LessonId = PaidLesson3Id,
            QuizId = Quiz3Id
        };

        await context.Lessons.AddRangeAsync(paidLesson1, paidLesson2, paidLesson3);
        await context.LessonVideos.AddAsync(paidVideo1);
        await context.LessonTexts.AddAsync(paidText2);
        await context.LessonQuizzes.AddAsync(paidQuiz3);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLessonsAsync(CatalogDbContext context)
    {
        // ============ SECTION 1: Introduction ============
        // Lesson 1.1 - Video (Preview)
        var lesson1_1 = new Lesson
        {
            Id = Lesson1_1Id,
            SectionId = Section1Id,
            Title = "Giới thiệu khóa học và lộ trình học tập",
            Description = "Tổng quan về khóa học, mục tiêu và cách tiếp cận hiệu quả nhất",
            Type = LessonType.Video,
            OrderIndex = 1,
            IsPreview = true,
            IsPublished = true,
            TotalViews = 5420,
            TotalCompletions = 4800,
            CreatedAt = DateTime.UtcNow.AddDays(-54)
        };

        var video1_1 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson1_1Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 900,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = false
        };

        // Lesson 1.2 - Text
        var lesson1_2 = new Lesson
        {
            Id = Lesson1_2Id,
            SectionId = Section1Id,
            Title = "Cài đặt môi trường phát triển",
            Description = "Hướng dẫn chi tiết cài đặt .NET SDK, Visual Studio và các công cụ cần thiết",
            Type = LessonType.Text,
            OrderIndex = 2,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 4200,
            TotalCompletions = 3900,
            CreatedAt = DateTime.UtcNow.AddDays(-53)
        };

        var text1_2 = new LessonText
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson1_2Id,
            TextContent = GetInstallationGuideContent()
        };

        // Lesson 1.3 - Video
        var lesson1_3 = new Lesson
        {
            Id = Lesson1_3Id,
            SectionId = Section1Id,
            Title = "Cấu trúc project ASP.NET Core",
            Description = "Hiểu về cấu trúc thư mục và các file quan trọng trong project",
            Type = LessonType.Video,
            OrderIndex = 3,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 3800,
            TotalCompletions = 3500,
            CreatedAt = DateTime.UtcNow.AddDays(-52)
        };

        var video1_3 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson1_3Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 1800,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = true
        };

        // ============ SECTION 2: Core Concepts ============
        // Lesson 2.1 - Video
        var lesson2_1 = new Lesson
        {
            Id = Lesson2_1Id,
            SectionId = Section2Id,
            Title = "Dependency Injection trong ASP.NET Core",
            Description = "Tìm hiểu DI Container, Service Lifetime và cách đăng ký services",
            Type = LessonType.Video,
            OrderIndex = 1,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 3200,
            TotalCompletions = 2900,
            CreatedAt = DateTime.UtcNow.AddDays(-48)
        };

        var video2_1 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson2_1Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 2700,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = true
        };

        // Lesson 2.2 - Text
        var lesson2_2 = new Lesson
        {
            Id = Lesson2_2Id,
            SectionId = Section2Id,
            Title = "Middleware Pipeline",
            Description = "Hiểu về request pipeline và cách tạo custom middleware",
            Type = LessonType.Text,
            OrderIndex = 2,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 2800,
            TotalCompletions = 2500,
            CreatedAt = DateTime.UtcNow.AddDays(-46)
        };

        var text2_2 = new LessonText
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson2_2Id,
            TextContent = GetMiddlewarePipelineContent()
        };

        // Lesson 2.3 - Video
        var lesson2_3 = new Lesson
        {
            Id = Lesson2_3Id,
            SectionId = Section2Id,
            Title = "Configuration và Options Pattern",
            Description = "Quản lý cấu hình ứng dụng với appsettings.json và Options Pattern",
            Type = LessonType.Video,
            OrderIndex = 3,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 2500,
            TotalCompletions = 2200,
            CreatedAt = DateTime.UtcNow.AddDays(-44)
        };

        var video2_3 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson2_3Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 2400,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = true
        };

        // Lesson 2.4 - Quiz
        var lesson2_4 = new Lesson
        {
            Id = Lesson2_4Id,
            SectionId = Section2Id,
            Title = "Quiz: Kiểm tra kiến thức Section 2",
            Description = "Kiểm tra hiểu biết về DI, Middleware và Configuration",
            Type = LessonType.Quiz,
            OrderIndex = 4,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 2100,
            TotalCompletions = 1800,
            CreatedAt = DateTime.UtcNow.AddDays(-42)
        };

        var quiz2_4 = new LessonQuiz
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson2_4Id,
            QuizId = Quiz1Id
        };

        // ============ SECTION 3: Entity Framework Core ============
        // Lesson 3.1 - Video
        var lesson3_1 = new Lesson
        {
            Id = Lesson3_1Id,
            SectionId = Section3Id,
            Title = "Giới thiệu Entity Framework Core",
            Description = "Tổng quan về ORM, DbContext và Entity Configuration",
            Type = LessonType.Video,
            OrderIndex = 1,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 2000,
            TotalCompletions = 1700,
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var video3_1 = new LessonVideo
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson3_1Id,
            VideoOriginalUrl = SeedVideoUrl,
            VideoThumbnailUrl = SeedImageUrl,
            DurationSeconds = 3600,
            HlsVariants = $"{{\"1080p\": \"{SeedVideoUrl}\"}}",
            VideoQualities = "[\"1080p\"]",
            IsDownloadable = true
        };

        // Lesson 3.2 - Text
        var lesson3_2 = new Lesson
        {
            Id = Lesson3_2Id,
            SectionId = Section3Id,
            Title = "Migrations và Database Management",
            Description = "Quản lý schema database với EF Core Migrations",
            Type = LessonType.Text,
            OrderIndex = 2,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 1800,
            TotalCompletions = 1500,
            CreatedAt = DateTime.UtcNow.AddDays(-38)
        };

        var text3_2 = new LessonText
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson3_2Id,
            TextContent = GetMigrationsContent()
        };

        // Lesson 3.3 - Quiz
        var lesson3_3 = new Lesson
        {
            Id = Lesson3_3Id,
            SectionId = Section3Id,
            Title = "Quiz: Entity Framework Core",
            Description = "Kiểm tra kiến thức về EF Core, DbContext và Migrations",
            Type = LessonType.Quiz,
            OrderIndex = 3,
            IsPreview = false,
            IsPublished = true,
            TotalViews = 1600,
            TotalCompletions = 1300,
            CreatedAt = DateTime.UtcNow.AddDays(-36)
        };

        var quiz3_3 = new LessonQuiz
        {
            Id = Guid.NewGuid(),
            LessonId = Lesson3_3Id,
            QuizId = Quiz2Id
        };

        // Add all lessons
        await context.Lessons.AddRangeAsync(
            lesson1_1, lesson1_2, lesson1_3,
            lesson2_1, lesson2_2, lesson2_3, lesson2_4,
            lesson3_1, lesson3_2, lesson3_3
        );

        // Add all lesson type-specific data
        await context.LessonVideos.AddRangeAsync(
            video1_1, video1_3,
            video2_1, video2_3,
            video3_1
        );

        await context.LessonTexts.AddRangeAsync(
            text1_2, text2_2, text3_2
        );

        await context.LessonQuizzes.AddRangeAsync(
            quiz2_4, quiz3_3
        );
    }

    private static string GetInstallationGuideContent()
    {
        return @"# Cài đặt môi trường phát triển ASP.NET Core

## 1. Cài đặt .NET SDK

Truy cập [dotnet.microsoft.com](https://dotnet.microsoft.com/download) và tải về phiên bản .NET SDK mới nhất (8.0 trở lên).

### Windows
```bash
winget install Microsoft.DotNet.SDK.8
```

### macOS
```bash
brew install dotnet-sdk
```

### Linux (Ubuntu)
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

## 2. Cài đặt IDE

### Visual Studio 2022 (Recommended for Windows)
- Tải từ [visualstudio.microsoft.com](https://visualstudio.microsoft.com/)
- Chọn workload ""ASP.NET and web development""

### Visual Studio Code
- Tải từ [code.visualstudio.com](https://code.visualstudio.com/)
- Cài đặt extensions:
  - C# Dev Kit
  - .NET Extension Pack

## 3. Kiểm tra cài đặt

```bash
dotnet --version
dotnet --list-sdks
```

## 4. Tạo project đầu tiên

```bash
dotnet new webapi -n MyFirstApi
cd MyFirstApi
dotnet run
```

Truy cập `https://localhost:5001/swagger` để xem API documentation.";
    }

    private static string GetMiddlewarePipelineContent()
    {
        return @"# Middleware Pipeline trong ASP.NET Core

## 1. Middleware là gì?

Middleware là các component được thực thi theo thứ tự trong request pipeline. Mỗi middleware có thể:
- Xử lý request trước khi chuyển tiếp
- Xử lý response sau khi nhận từ middleware tiếp theo
- Short-circuit pipeline (không chuyển tiếp request)

## 2. Request Pipeline

```
Request → Middleware 1 → Middleware 2 → ... → Endpoint
                ↓              ↓
Response ← Middleware 1 ← Middleware 2 ← ...
```

## 3. Built-in Middleware

```csharp
var app = builder.Build();

app.UseExceptionHandler(""/Error"");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

## 4. Custom Middleware

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation(""Request {Method} {Path} completed in {ElapsedMs}ms"",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds);
    }
}
```

## 5. Thứ tự Middleware quan trọng

1. Exception Handler (đầu tiên để catch mọi exception)
2. HSTS
3. HTTPS Redirection
4. Static Files
5. Routing
6. CORS
7. Authentication
8. Authorization
9. Custom Middleware
10. Endpoints (cuối cùng)";
    }

    private static string GetMigrationsContent()
    {
        return @"# EF Core Migrations

## 1. Tạo Migration

```bash
# Sử dụng .NET CLI
dotnet ef migrations add InitialCreate

# Sử dụng Package Manager Console
Add-Migration InitialCreate
```

## 2. Áp dụng Migration

```bash
# Áp dụng tất cả migrations pending
dotnet ef database update

# Áp dụng đến migration cụ thể
dotnet ef database update InitialCreate
```

## 3. Rollback Migration

```bash
# Rollback về migration trước
dotnet ef database update PreviousMigration

# Rollback về trạng thái ban đầu (empty database)
dotnet ef database update 0
```

## 4. Remove Migration

```bash
# Xóa migration cuối cùng (chưa apply)
dotnet ef migrations remove
```

## 5. Script Migration

```bash
# Generate SQL script
dotnet ef migrations script

# Script từ migration A đến B
dotnet ef migrations script MigrationA MigrationB
```

## 6. Best Practices

1. **Đặt tên migration có ý nghĩa**: `AddUserEmailIndex`, `RemoveObsoleteColumn`
2. **Kiểm tra SQL trước khi apply**: `dotnet ef migrations script`
3. **Không sửa migration đã apply**: Tạo migration mới thay vì sửa
4. **Seed data trong migration**: Sử dụng `migrationBuilder.InsertData()`
5. **Backup trước khi apply trên production**";
    }
}