using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data
{
    public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : BaseDbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<InstructorProfile> InstructorProfiles { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
                {
                    entity.HasQueryFilter(e => e.DeletedAt == null);
                    entity.HasMany(u => u.UserRoles)
                        .WithOne(ur => ur.User)
                        .HasForeignKey(ur => ur.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                    entity.HasMany(u => u.UserSubscriptions)
                        .WithOne(s => s.User)
                        .HasForeignKey(s => s.UserId)
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
            modelBuilder.Entity<SubscriptionPlan>(entity =>
                {
                    entity.HasQueryFilter(e => e.DeletedAt == null);
                    entity.HasIndex(p => p.Code).IsUnique();
                    entity.Property(p => p.Price).HasPrecision(18, 2);
                });
            modelBuilder.Entity<UserSubscription>(entity =>
                {
                    entity.HasQueryFilter(e => e.DeletedAt == null);
                    entity.HasOne(s => s.Plan)
                        .WithMany(p => p.UserSubscriptions)
                        .HasForeignKey(s => s.PlanId)
                        .OnDelete(DeleteBehavior.Restrict);
                    entity.HasIndex(s => new { s.UserId, s.Status });
                });
        }
    }
}
