using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using System;
using System.Collections.Generic;

using Up4All.Framework.MessageBus.Kafka.HealthChecks;
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
            // Only registers this library's ActivitySource so the tracer picks up its spans.
            // Resource attributes (service.name/service.version) belong to the host
            // application - setting them here would overwrite whatever the app configured.
            return builder.AddSource(KafkaExtensions.ActivitySource.Name);
        }

        public static MeterProviderBuilder AddOpenTelemetryMetricsForMessageBus(this MeterProviderBuilder builder)
        {
            // Only registers this library's Meter so the MeterProvider picks up its
            // instruments (messaging.client.sent.messages / .consumed.messages /
            // .operation.duration). Resource attributes are the host application's concern.
            return builder.AddMeter(KafkaExtensions.Meter.Name);
        }

        /// <summary>
        /// Registers a health check that reports whether the Kafka cluster is reachable, via
        /// AdminClient.GetMetadata against the given bootstrap servers. Unlike the
        /// RabbitMQ/ServiceBus health checks this performs real (if cheap) network I/O, since
        /// Confluent.Kafka's producer/consumer expose no simple "is connected" flag to read.
        /// </summary>
        public static IHealthChecksBuilder AddKafkaMessageBusHealthCheck(this IHealthChecksBuilder builder
            , string bootstrapServers
            , TimeSpan? timeout = null
            , string name = "kafka-messagebus"
            , HealthStatus? failureStatus = null
            , IEnumerable<string>? tags = null)
        {
            return builder.Add(new HealthCheckRegistration(name
                , _ => new KafkaMessageBusHealthCheck(bootstrapServers, timeout)
                , failureStatus, tags));
        }
    }
}
