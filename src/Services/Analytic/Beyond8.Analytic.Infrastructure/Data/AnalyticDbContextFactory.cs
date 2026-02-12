using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Analytic.Infrastructure.Data;

public class AnalyticDbContextFactory : IDesignTimeDbContextFactory<AnalyticDbContext>
{
    public AnalyticDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=analytic_db;Username=postgres;Password=postgres");
        return new AnalyticDbContext(optionsBuilder.Options);
    }
}
