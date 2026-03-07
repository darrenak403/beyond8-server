using Beyond8.Common.Data.Base;
using Beyond8.Learning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Learning.Infrastructure.Data;

public class LearningDbContext(DbContextOptions<LearningDbContext> options) : BaseDbContext(options)
{
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<LessonProgress> LessonProgresses { get; set; } = null!;
    public DbSet<SectionProgress> SectionProgresses { get; set; } = null!;
    public DbSet<CourseReview> CourseReviews { get; set; } = null!;
    public DbSet<Certificate> Certificates { get; set; } = null!;
    public DbSet<CourseCertificateEligibilityConfig> CourseCertificateEligibilityConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.InstructorId);
            entity.Property(e => e.PricePaid).HasPrecision(18, 2);
            entity.Property(e => e.ProgressPercent).HasPrecision(5, 2);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
            entity.HasIndex(e => e.EnrollmentId);
            entity.HasIndex(e => new { e.CourseId, e.UserId });
            entity.Property(e => e.WatchPercent).HasPrecision(5, 2);
            entity.Property(e => e.QuizBestScore).HasPrecision(5, 2);
            entity.HasOne(e => e.Enrollment)
                .WithMany(en => en.LessonProgresses)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SectionProgress>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => new { e.UserId, e.SectionId }).IsUnique();
            entity.HasIndex(e => e.EnrollmentId);
            entity.Property(e => e.AssignmentGrade).HasPrecision(5, 2);
            entity.HasOne(e => e.Enrollment)
                .WithMany(en => en.SectionProgresses)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            entity.HasIndex(e => e.CourseId);
            entity.HasOne(e => e.Enrollment)
                .WithOne(en => en.Review)
                .HasForeignKey<CourseReview>(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.CertificateNumber).IsUnique();
            entity.HasIndex(e => e.EnrollmentId).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VerificationHash);
            entity.HasOne(e => e.Enrollment)
                .WithOne(en => en.Certificate)
                .HasForeignKey<Certificate>(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseCertificateEligibilityConfig>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.CourseId).IsUnique();
            entity.Property(e => e.QuizAverageMinPercent).HasPrecision(5, 2);
            entity.Property(e => e.AssignmentAverageMinPercent).HasPrecision(5, 2);
        });
    }
}
