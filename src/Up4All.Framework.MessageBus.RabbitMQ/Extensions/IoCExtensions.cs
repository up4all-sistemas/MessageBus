using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

using Up4All.Framework.MessageBus.RabbitMQ.Pipelines;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class IoCExtensions
    {
        public static RabbitMQMessageBusPipeline AddRabbitMQMessageBus(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            return new RabbitMQMessageBusPipeline(services, configurationBindKey);
        }

        public static TracerProviderBuilder AddOpenTelemetryForRabbitMQMessageBus(this TracerProviderBuilder builder)
        {
            builder.AddSource(RabbitMQClientExtensions.ActivitySource.Name);
            return builder;
        }

    }
}
