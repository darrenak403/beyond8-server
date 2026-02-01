using Beyond8.Catalog.Domain.Entities;
using Beyond8.Common.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data
{
    public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : BaseDbContext(options)
    {
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

                // Configure SearchVector for full-text search
                entity.Property(c => c.SearchVector)
                    .HasColumnType("tsvector")
                    .ValueGeneratedOnAddOrUpdate();

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
