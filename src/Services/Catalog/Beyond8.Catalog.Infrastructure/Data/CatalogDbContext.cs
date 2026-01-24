using System;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Common.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : BaseDbContext(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Section> Sections { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<CourseDocument> CourseDocuments { get; set; } = null!;
    public DbSet<LessonDocument> LessonDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
        modelBuilder.Entity<Lesson>(entity =>
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
