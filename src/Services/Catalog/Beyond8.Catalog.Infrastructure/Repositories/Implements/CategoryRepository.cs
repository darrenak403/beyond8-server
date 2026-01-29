using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Catalog.Infrastructure.Repositories.Implements
{
    public class CategoryRepository(CatalogDbContext context) : PostgresRepository<Category>(context), ICategoryRepository
    {

    }
}
