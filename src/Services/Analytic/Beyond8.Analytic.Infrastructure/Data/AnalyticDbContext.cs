using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Data;

public class AnalyticDbContext(DbContextOptions<AnalyticDbContext> options) : BaseDbContext(options)
{
    public DbSet<AggCourseStats> AggCourseStats { get; set; } = null!;
    public DbSet<AggLessonPerformance> AggLessonPerformances { get; set; } = null!;
    public DbSet<AggInstructorRevenue> AggInstructorRevenues { get; set; } = null!;
    public DbSet<AggSystemOverview> AggSystemOverviews { get; set; } = null!;
    public DbSet<AggSystemOverviewMonthly> AggSystemOverviewMonthlies { get; set; } = null!;
    public DbSet<AggSystemOverviewDaily> AggSystemOverviewDailies { get; set; } = null!;
    public DbSet<AggAiUsageDaily> AggAiUsageDailies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggCourseStats>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.SnapshotDate);
            entity.HasIndex(e => e.IsCurrent);
        });

        modelBuilder.Entity<AggLessonPerformance>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.SnapshotDate);
            entity.HasIndex(e => e.IsCurrent);
        });

        modelBuilder.Entity<AggInstructorRevenue>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.SnapshotDate);
            entity.HasIndex(e => e.IsCurrent);
        });

        modelBuilder.Entity<AggSystemOverview>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.IsCurrent);
            entity.HasIndex(e => e.SnapshotDate);
        });

        modelBuilder.Entity<AggSystemOverviewMonthly>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.YearMonth).IsUnique();
            entity.HasIndex(e => new { e.Year, e.Month });
        });

        modelBuilder.Entity<AggSystemOverviewDaily>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.DateKey).IsUnique();
            entity.HasIndex(e => new { e.Year, e.Month, e.Day });
        });

        modelBuilder.Entity<AggAiUsageDaily>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.SnapshotDate);
            entity.HasIndex(e => new { e.SnapshotDate, e.Model, e.Provider }).IsUnique();
        });
    }
}
