using System;
using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : BaseDbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<InstructorProfile> InstructorProfiles { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                entity.HasMany(u => u.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        modelBuilder.Entity<Role>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                entity.HasIndex(r => r.Code).IsUnique();
                entity.HasMany(r => r.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            });
        modelBuilder.Entity<InstructorProfile>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
    }
}
