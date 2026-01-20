using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : BaseDbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<InstructorProfile> InstructorProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        modelBuilder.Entity<InstructorProfile>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
    }
}
