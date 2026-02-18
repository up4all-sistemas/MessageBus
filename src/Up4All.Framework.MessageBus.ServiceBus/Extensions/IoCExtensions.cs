using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Up4All.Framework.MessageBus.ServiceBus.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        public static ServiceBusMessageBusPipeline AddServiceBusMessageBus(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            return new ServiceBusMessageBusPipeline(services, configurationBindKey);
        }

        public static TracerProviderBuilder AddOpenTelemetryForServiceBusMessageBus(this TracerProviderBuilder builder)
        {
            var resBuilder = ResourceBuilder.CreateDefault()
                .AddService(ServiceBusClientExtensions.ActivitySource.Name, serviceVersion: ServiceBusClientExtensions.ActivitySource.Version);

            builder.AddSource(ServiceBusClientExtensions.ActivitySource.Name)
                .SetResourceBuilder(resBuilder);

            return builder;
        }
    }
}
