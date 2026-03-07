using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Sale.Infrastructure.Data;

public class SaleDbContextFactory : IDesignTimeDbContextFactory<SaleDbContext>
{
    public SaleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SaleDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=sale_db;Username=postgres;Password=postgres");
        return new SaleDbContext(optionsBuilder.Options);
    }
}
