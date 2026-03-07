using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Common.Extensions
{
    public static class MassTransitExtensions
    {
        /// <summary>
        /// Adds MassTransit with RabbitMQ. Use <paramref name="queueNamePrefix"/> so each service
        /// gets its own queues; otherwise multiple services with the same consumer type share one queue
        /// and only one service receives each message.
        /// </summary>
        /// <param name="queueNamePrefix">Prefix for queue names (e.g. "catalog", "identity"). Must be unique per service.</param>
        public static IHostApplicationBuilder AddMassTransitWithRabbitMq(
            this IHostApplicationBuilder builder,
            Action<IBusRegistrationConfigurator>? configurator = null,
            string? queueNamePrefix = null)
        {
            builder.Services.AddMassTransit(x =>
            {
                if (!string.IsNullOrWhiteSpace(queueNamePrefix))
                {
                    var formatter = new KebabCaseEndpointNameFormatter(queueNamePrefix.Trim(), includeNamespace: false);
                    x.SetEndpointNameFormatter(formatter);
                }

                configurator?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")
                        ?? throw new InvalidOperationException("RabbitMQ connection string 'rabbitmq' is not configured");

                    cfg.Host(rabbitMqConnectionString);

                    if (!string.IsNullOrWhiteSpace(queueNamePrefix))
                    {
                        var formatter = new KebabCaseEndpointNameFormatter(queueNamePrefix.Trim(), includeNamespace: false);
                        cfg.ConfigureEndpoints(context, formatter);
                    }
                    else
                    {
                        cfg.ConfigureEndpoints(context);
                    }

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
