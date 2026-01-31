using Microsoft.EntityFrameworkCore;

namespace Beyond8.Common.Data.Interfaces
{
    public interface IBaseUnitOfWork
    {
        DbContext Context { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task ExecuteInTransactionAsync(Func<Task> operation);
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
    }
}
