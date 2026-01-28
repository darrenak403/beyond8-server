using Beyond8.Common.Caching;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Beyond8.Common.Extensions
{
    public static class CacheExtensions
    {
        private const string DefaultConnectionName = Const.Redis;

        public static IHostApplicationBuilder AddServiceRedis(this IHostApplicationBuilder builder, string serviceName, string connectionName = DefaultConnectionName)
        {
            builder.AddRedisClient(connectionName);

            builder.Services.AddSingleton<ICacheService>(sp =>
            {
                var connection = sp.GetRequiredService<IConnectionMultiplexer>();
                var db = connection.GetDatabase();

                return new CacheService(db, serviceName);
            });

            return builder;
        }
    }
}