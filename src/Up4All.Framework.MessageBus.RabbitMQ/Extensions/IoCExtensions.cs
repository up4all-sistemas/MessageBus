using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using System;
using System.Collections.Generic;

using Up4All.Framework.MessageBus.RabbitMQ.HealthChecks;
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
            // Only registers this library's ActivitySource so the tracer picks up its spans.
            // Resource attributes (service.name/service.version) belong to the host
            // application - setting them here would overwrite whatever the app configured.
            return builder.AddSource(RabbitMQClientExtensions.ActivitySource.Name);
        }

        public static MeterProviderBuilder AddOpenTelemetryMetricsForRabbitMQMessageBus(this MeterProviderBuilder builder)
        {
            // Only registers this library's Meter so the MeterProvider picks up its
            // instruments (messaging.client.sent.messages / .consumed.messages /
            // .operation.duration). Resource attributes are the host application's concern.
            return builder.AddMeter(RabbitMQClientExtensions.Meter.Name);
        }

        /// <summary>
        /// Registers a health check that reports whether the given RabbitMQ client's
        /// connection is open. The host supplies the accessor since the library may have
        /// several independent clients registered (e.g. via keyed AddPublisher/AddConsumer
        /// overloads) - there is no single implicit "the" connection to default to.
        /// </summary>
        public static IHealthChecksBuilder AddRabbitMQMessageBusHealthCheck(this IHealthChecksBuilder builder
            , Func<IServiceProvider, IRabbitMQClient> clientAccessor
            , string name = "rabbitmq-messagebus"
            , HealthStatus? failureStatus = null
            , IEnumerable<string>? tags = null)
        {
            return builder.Add(new HealthCheckRegistration(name
                , sp => new RabbitMQMessageBusHealthCheck(() => clientAccessor(sp))
                , failureStatus, tags));
        }
    }
}
