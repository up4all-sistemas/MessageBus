using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.Kafka.HealthChecks
{
    /// <summary>
    /// Reports whether the Kafka cluster is reachable. Unlike the RabbitMQ/Service Bus health
    /// checks, this cannot just read a boolean off an existing client - Confluent.Kafka's
    /// producer/consumer don't expose a connection-open flag - so it makes a real (but cheap)
    /// AdminClient.GetMetadata call against the broker. It builds a short-lived AdminClient
    /// per check rather than reusing a registered producer/consumer, so it works regardless of
    /// how many topic/subscription clients the host has registered.
    /// </summary>
    public class KafkaMessageBusHealthCheck(string bootstrapServers, TimeSpan? timeout = null) : IHealthCheck
    {
        private readonly TimeSpan _timeout = timeout ?? TimeSpan.FromSeconds(5);

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
                var metadata = adminClient.GetMetadata(_timeout);

                return Task.FromResult(HealthCheckResult.Healthy($"Kafka cluster reachable ({metadata.Brokers.Count} broker(s))."));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka cluster is not reachable.", ex));
            }
        }
    }
}
