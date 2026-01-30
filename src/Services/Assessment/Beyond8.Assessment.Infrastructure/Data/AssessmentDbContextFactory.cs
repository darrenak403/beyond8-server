using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Assessment.Infrastructure.Data;

public class AssessmentDbContextFactory : IDesignTimeDbContextFactory<AssessmentDbContext>
{
    public AssessmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssessmentDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=assessment_db;Username=postgres;Password=postgres");
        return new AssessmentDbContext(optionsBuilder.Options);
    }
}
