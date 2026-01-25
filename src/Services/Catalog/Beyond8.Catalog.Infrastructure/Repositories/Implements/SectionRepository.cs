using System;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Infrastructure.Repositories.Implements;

public class SectionRepository(CatalogDbContext context) : PostgresRepository<Section>(context), ISectionRepository
{

}
