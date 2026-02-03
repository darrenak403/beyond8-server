using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Learning.Infrastructure.Data;

public class LearningDbContextFactory : IDesignTimeDbContextFactory<LearningDbContext>
{
    public LearningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LearningDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=learning_db;Username=postgres;Password=postgres");
        return new LearningDbContext(optionsBuilder.Options);
    }
}
