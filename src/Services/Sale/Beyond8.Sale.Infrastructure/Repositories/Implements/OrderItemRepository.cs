using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Sale.Infrastructure.Repositories.Implements;

public class OrderItemRepository(SaleDbContext context) : PostgresRepository<OrderItem>(context), IOrderItemRepository;
