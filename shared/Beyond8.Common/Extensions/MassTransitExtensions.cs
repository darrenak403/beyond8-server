using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Common.Extensions
{
    public static class MassTransitExtensions
    {
        public static IHostApplicationBuilder AddMassTransitWithRabbitMq(
            this IHostApplicationBuilder builder,
            Action<IBusRegistrationConfigurator>? configurator = null)
        {
            builder.Services.AddMassTransit(x =>
            {
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
