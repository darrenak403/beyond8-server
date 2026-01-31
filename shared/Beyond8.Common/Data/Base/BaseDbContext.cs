using Beyond8.Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Common.Data.Base
{
    public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = entry.Entity.Id;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = entry.Entity.Id;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
