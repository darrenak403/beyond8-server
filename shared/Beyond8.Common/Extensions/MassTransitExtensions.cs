using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Common.Extensions
{
    public static class MassTransitExtensions
    {
        /// <summary>
        /// Adds MassTransit with RabbitMQ configuration.
        /// Use configurator action to register consumers for each service.
        /// </summary>
        public static IHostApplicationBuilder AddMassTransitWithRabbitMq(
            this IHostApplicationBuilder builder,
            Action<IBusRegistrationConfigurator>? configurator = null)
        {
            builder.Services.AddMassTransit(x =>
            {
                // Allow each service to register its own consumers
                configurator?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")
                        ?? throw new InvalidOperationException("RabbitMQ connection string 'rabbitmq' is not configured");

                    cfg.Host(rabbitMqConnectionString);

                    cfg.ConfigureEndpoints(context);

                    // Configure retry policy
                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5)
                        );
                    });
                });
            });

            return builder;
        }
    }
}
