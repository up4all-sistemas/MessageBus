using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.RabbitMQ.HealthChecks
{
    /// <summary>
    /// Reports whether an already-established RabbitMQ connection is open. It never opens a
    /// connection itself - just reads IConnection.IsOpen off whichever client the host points
    /// it at - so it stays cheap enough to poll frequently and never performs network I/O of
    /// its own. Because the library connects lazily (on first publish/consume), this reports
    /// Unhealthy both when the connection has genuinely dropped and when it simply hasn't been
    /// used yet; the two aren't distinguishable from IConnection.IsOpen alone.
    /// </summary>
    public class RabbitMQMessageBusHealthCheck(Func<IRabbitMQClient?> clientAccessor) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var client = clientAccessor();
            var isOpen = client?.Connection?.IsOpen ?? false;

            var result = isOpen
                ? HealthCheckResult.Healthy("RabbitMQ connection is open.")
                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed or has not been established yet.");

            return Task.FromResult(result);
        }
    }
}
