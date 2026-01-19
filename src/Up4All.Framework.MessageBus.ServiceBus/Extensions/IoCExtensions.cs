using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.ServiceBus.Consts;
using Up4All.Framework.MessageBus.ServiceBus.Consumers;
using Up4All.Framework.MessageBus.ServiceBus.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        public static ServiceBusMessageBusPipeline AddServiceBusMessageBus(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            return new ServiceBusMessageBusPipeline(services, configurationBindKey);
        }

        public static TracerProviderBuilder AddOpenTelemetryForMessageBus(this TracerProviderBuilder builder)
        {
            builder.AddSource(ServiceBusClientExtensions.ActivitySource.Name);
            return builder;
        }
    }
}
