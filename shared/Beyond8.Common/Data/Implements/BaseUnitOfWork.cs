using Beyond8.Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Beyond8.Common.Data.Implements;

public abstract class BaseUnitOfWork<TContext>(TContext context) : IBaseUnitOfWork where TContext : DbContext
{
    protected readonly TContext Context = context;
    private IDbContextTransaction? _transaction;

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task BeginTransactionAsync()
    {
        _transaction = await Context.Database.BeginTransactionAsync();
    }

    public virtual async Task CommitTransactionAsync()
    {
        try
        {
            await Context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public virtual async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public virtual void Dispose()
    {
        _transaction?.Dispose();
        Context.Dispose();
    }
}