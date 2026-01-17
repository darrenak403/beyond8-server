using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beyond8.DatabaseMigrationHelpers
{
    public static class DatabaseMigration
    {
        public static async Task<IHost> MigrateDbContextAsync<TContext>(this IHost host, Func<DatabaseFacade, CancellationToken?, Task>? postMigration = null, CancellationToken cancellationToken = default) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetRequiredService<TContext>();
            if (context is not null)
            {
                try
                {
                    var dbCreator = context.Database.GetService<IRelationalDatabaseCreator>();
                    var strategy = context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        if (!await dbCreator.ExistsAsync(cancellationToken))
                        {
                            logger.LogInformation("Database does not exist. Creating database for context {DbContextName}", typeof(TContext).Name);
                            await dbCreator.CreateAsync(cancellationToken);
                        }
                    });

                    logger.LogInformation("Migrating database for context {DbContextName}", typeof(TContext).Name);
                    await strategy.ExecuteAsync(async () =>
                    {
                        try
                        {
                            await context.Database.MigrateAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while applying migrations for context {DbContextName}", typeof(TContext).Name);
                            throw;
                        }
                    });

                    logger.LogInformation("Database migration completed for context {DbContextName}", typeof(TContext).Name);
                    if (postMigration is not null)
                    {
                        try
                        {
                            logger.LogInformation("Executing post-migration action for context {DbContextName}", typeof(TContext).Name);
                            await postMigration(context.Database, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while executing the post-migration action for context {DbContextName}", typeof(TContext).Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }
            return host;
        }
    }
}