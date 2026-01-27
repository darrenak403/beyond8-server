using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Identity.Infrastructure.Data
{
    public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        public IdentityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=Identity;Username=postgres;Password=postgres",
                options => options.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName));
            return new IdentityDbContext(optionsBuilder.Options);
        }
    }
}
