using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Up4All.Framework.MessageBus.Kafka.Pipelines;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class IoCExtensions
    {
        public static KafkaMessageBusPipeline AddKafkaMessageBus(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            return new KafkaMessageBusPipeline(services, configurationBindKey);
        }

        public static TracerProviderBuilder AddOpenTelemetryForMessageBus(this TracerProviderBuilder builder)
        {
            var resBuilder = ResourceBuilder.CreateDefault()
                .AddService(KafkaExtensions.ActivitySource.Name, serviceVersion: KafkaExtensions.ActivitySource.Version);

            builder.AddSource(KafkaExtensions.ActivitySource.Name)
                .SetResourceBuilder(resBuilder);
            
            return builder;
        }
    }
}
