using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Kafka.HealthChecks;

namespace Up4All.Framework.MessageBus.Kafka.Tests.HealthChecks
{
    [TestFixture]
    public class KafkaMessageBusHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_UnreachableBroker_ReturnsUnhealthyWithException()
        {
            // No real broker is running at this address, and a short timeout keeps the test
            // fast - GetMetadata is expected to fail, which is exactly the path being tested.
            var healthCheck = new KafkaMessageBusHealthCheck("localhost:1", TimeSpan.FromMilliseconds(200));

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Exception, Is.Not.Null);
        }
    }
}
