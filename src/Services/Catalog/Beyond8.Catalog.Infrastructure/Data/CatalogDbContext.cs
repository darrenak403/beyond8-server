using Beyond8.Catalog.Application.Helpers;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Data.Base;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Beyond8.Catalog.Infrastructure.Data
{
    public class CatalogDbContext : BaseDbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }
        private const string SearchVectorUpdateFunction = "courses_search_vector_update_for_ids";
        private const int SearchableTextMaxLength = 10000;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var courseEntries = ChangeTracker
                .Entries<Course>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            if (courseEntries.Count > 0)
            {
                var categoryIds = courseEntries.Select(e => e.Entity.CategoryId).Distinct().ToList();
                // Do not pass cancellationToken into EF methods here to avoid provider treating it as a mappable parameter
                var categories = await Set<Category>()
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, c => c.Name);

                foreach (var entry in courseEntries)
                {
                    var c = entry.Entity;
                    var categoryName = categories.GetValueOrDefault(c.CategoryId);
                    c.SearchableText = BuildSearchableText(c, categoryName);
                }
            }

            var courseIds = courseEntries.Select(e => e.Entity.Id).Distinct().ToList();
            var count = await base.SaveChangesAsync(cancellationToken);

            if (courseIds.Count > 0)
            {
                await Database.ExecuteSqlRawAsync(
                    $"SELECT {SearchVectorUpdateFunction}(@p0)",
                    new NpgsqlParameter
                    {
                        ParameterName = "p0",
                        NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid,
                        Value = courseIds.ToArray()
                    });
            }

            return count;
        }

        /// <summary>
        /// Builds normalized text for full-text search so Vietnamese without diacritics (e.g. "lap trinh") matches text with diacritics (e.g. "Lập trình").
        /// </summary>
        private static string BuildSearchableText(Course c, string? categoryName)
        {
            var parts = new List<string>
            {
                c.Title,
                c.Slug,
                c.ShortDescription ?? "",
                c.Description,
                categoryName ?? "",
                FlattenJsonForSearch(c.Outcomes),
                FlattenJsonForSearch(c.Requirements ?? "[]"),
                FlattenJsonForSearch(c.TargetAudience ?? "[]"),
                GetStatusSearchText(c.Status),
                GetLevelSearchText(c.Level),
                c.Language,
                c.InstructorName
            };

            var combined = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            var normalized = StringHelper.RemoveDiacritics(combined);
            if (normalized.Length > SearchableTextMaxLength)
                normalized = normalized[..SearchableTextMaxLength];
            return normalized;
        }

        private static string FlattenJsonForSearch(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "[]") return "";
            // Simple flatten: remove JSON brackets and quotes so words are searchable
            return System.Text.RegularExpressions.Regex.Replace(json, @"[\[\]""\\,]", " ");
        }

        private static string GetStatusSearchText(CourseStatus status) => status switch
        {
            CourseStatus.Draft => "Draft Bản nháp",
            CourseStatus.PendingApproval => "PendingApproval Chờ duyệt Pending",
            CourseStatus.Approved => "Approved Đã duyệt",
            CourseStatus.Rejected => "Rejected Từ chối",
            CourseStatus.Published => "Published Công khai Đã xuất bản",
            CourseStatus.Archived => "Archived Lưu trữ",
            CourseStatus.Suspended => "Suspended Tạm ngưng",
            _ => ""
        };

        private static string GetLevelSearchText(CourseLevel level) => level switch
        {
            CourseLevel.Beginner => "Beginner Cơ bản Người mới",
            CourseLevel.Intermediate => "Intermediate Trung cấp Trung bình",
            CourseLevel.Advanced => "Advanced Nâng cao Chuyên sâu",
            CourseLevel.Expert => "Expert Chuyên gia",
            CourseLevel.All => "All Tất cả",
            _ => ""
        };
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<LessonVideo> LessonVideos { get; set; } = null!;
        public DbSet<LessonText> LessonTexts { get; set; } = null!;
        public DbSet<LessonQuiz> LessonQuizzes { get; set; } = null!;
        public DbSet<CourseDocument> CourseDocuments { get; set; } = null!;
        public DbSet<LessonDocument> LessonDocuments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enable unaccent extension for Vietnamese diacritics support
            modelBuilder.HasPostgresExtension("unaccent");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);

                entity.Property(c => c.SearchableText)
                    .HasMaxLength(SearchableTextMaxLength);

                // SearchVector updated from code in SaveChangesAsync via courses_search_vector_update_for_ids()
                entity.Property(c => c.SearchVector)
                    .HasColumnType("tsvector");

                // Create GIN index for fast full-text search
                entity.HasIndex(c => c.SearchVector)
                    .HasMethod("GIN");
            });
            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                entity.HasOne(l => l.Video).WithOne(v => v.Lesson).HasForeignKey<LessonVideo>(v => v.LessonId);
                entity.HasOne(l => l.Text).WithOne(t => t.Lesson).HasForeignKey<LessonText>(t => t.LessonId);
                entity.HasOne(l => l.Quiz).WithOne(q => q.Lesson).HasForeignKey<LessonQuiz>(q => q.LessonId);
            });
            modelBuilder.Entity<LessonVideo>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<LessonText>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<LessonQuiz>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<CourseDocument>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<LessonDocument>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
