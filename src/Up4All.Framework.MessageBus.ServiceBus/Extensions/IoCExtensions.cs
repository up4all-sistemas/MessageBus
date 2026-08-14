using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using System;
using System.Collections.Generic;

using Up4All.Framework.MessageBus.ServiceBus.HealthChecks;
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
            // Only registers this library's ActivitySource so the tracer picks up its spans.
            // Resource attributes (service.name/service.version) belong to the host
            // application - setting them here would overwrite whatever the app configured.
            return builder.AddSource(ServiceBusClientExtensions.ActivitySource.Name);
        }

        public static MeterProviderBuilder AddOpenTelemetryMetricsForServiceBusMessageBus(this MeterProviderBuilder builder)
        {
            // Only registers this library's Meter so the MeterProvider picks up its
            // instruments (messaging.client.sent.messages / .consumed.messages /
            // .operation.duration). Resource attributes are the host application's concern.
            return builder.AddMeter(ServiceBusClientExtensions.Meter.Name);
        }

        /// <summary>
        /// Registers a health check that reports whether the given Service Bus client's
        /// connection is open (ServiceBusClient.IsClosed). The host supplies the accessor
        /// since the library may have several independent clients registered - there is no
        /// single implicit "the" connection to default to.
        /// </summary>
        public static IHealthChecksBuilder AddServiceBusMessageBusHealthCheck(this IHealthChecksBuilder builder
            , Func<IServiceProvider, IServiceBusClient> clientAccessor
            , string name = "servicebus-messagebus"
            , HealthStatus? failureStatus = null
            , IEnumerable<string>? tags = null)
        {
            return builder.Add(new HealthCheckRegistration(name
                , sp => new ServiceBusMessageBusHealthCheck(() => clientAccessor(sp))
                , failureStatus, tags));
        }
    }
}
