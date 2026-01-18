using System;
using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Data;

public class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : BaseDbContext(options)
{
    public DbSet<MediaFile> MediaFiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
    }
}
