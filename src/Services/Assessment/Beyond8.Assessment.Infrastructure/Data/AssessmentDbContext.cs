using Beyond8.Assessment.Domain.Entities;
using Beyond8.Common.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Assessment.Infrastructure.Data;

public class AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : BaseDbContext(options)
{
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
    public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete query filters
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Index for instructor queries
            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.LessonId);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Composite index for quiz questions
            entity.HasIndex(e => new { e.QuizId, e.OrderIndex });
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.SectionId);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.QuizId);
            entity.HasIndex(e => new { e.QuizId, e.StudentId });
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<AssignmentSubmission>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => new { e.AssignmentId, e.StudentId });
            entity.HasIndex(e => e.Status);
        });
    }
}
