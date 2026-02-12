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
    }
}
