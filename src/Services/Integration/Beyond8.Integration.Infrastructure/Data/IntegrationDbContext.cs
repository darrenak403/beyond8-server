using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Data
{
    public class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : BaseDbContext(options)
    {
        public DbSet<MediaFile> MediaFiles { get; set; } = null!;
        public DbSet<AiUsage> AiUsages { get; set; } = null!;
        public DbSet<AiPrompt> AiPrompts { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            modelBuilder.Entity<AiUsage>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);

                entity.HasOne(e => e.Prompt)
                    .WithMany(p => p.Usages)
                    .HasForeignKey(e => e.PromptId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AiPrompt>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
