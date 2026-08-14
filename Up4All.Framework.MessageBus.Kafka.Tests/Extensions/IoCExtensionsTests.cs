using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Kafka.Extensions;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Extensions
{
    [TestFixture]
    public class IoCExtensionsTests
    {
        [Test]
        public void AddKafkaMessageBus_DefaultConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddKafkaMessageBus();

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }

        [Test]
        public void AddKafkaMessageBus_CustomConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddKafkaMessageBus("CustomKey");

            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("CustomKey"));
        }

        [Test]
        public void AddOpenTelemetryForMessageBus_BuildsProvider()
        {
            using var provider = Sdk.CreateTracerProviderBuilder()
                .AddOpenTelemetryForMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryForMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateTracerProviderBuilder();

            var result = builder.AddOpenTelemetryForMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void AddOpenTelemetryForMessageBus_DoesNotOverwriteHostServiceName()
        {
            // Regression test: this used to call .SetResourceBuilder(...) with the library's
            // own ActivitySource name, silently replacing whatever service.name the host
            // application had already configured.
            using var provider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("my-app"))
                .AddOpenTelemetryForMessageBus()
                .Build();

            var resource = provider!.GetResource();
            var serviceName = resource.Attributes.First(a => a.Key == "service.name").Value;

            Assert.That(serviceName, Is.EqualTo("my-app"));
        }

        [Test]
        public void AddOpenTelemetryMetricsForMessageBus_RegistersMeterAndBuildsProvider()
        {
            using var provider = Sdk.CreateMeterProviderBuilder()
                .AddOpenTelemetryMetricsForMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryMetricsForMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateMeterProviderBuilder();

            var result = builder.AddOpenTelemetryMetricsForMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public async Task AddKafkaMessageBusHealthCheck_RegistersCheckThatReportsUnreachableBroker()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHealthChecks()
                .AddKafkaMessageBusHealthCheck("localhost:1", TimeSpan.FromMilliseconds(200));

            using var provider = services.BuildServiceProvider();
            var healthCheckService = provider.GetRequiredService<HealthCheckService>();

            var report = await healthCheckService.CheckHealthAsync();

            Assert.That(report.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(report.Entries.ContainsKey("kafka-messagebus"), Is.True);
        }
    }
}
