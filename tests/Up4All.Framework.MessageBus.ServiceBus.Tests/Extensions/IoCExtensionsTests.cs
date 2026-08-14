using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.ServiceBus.Extensions;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Extensions
{
    [TestFixture]
    public class IoCExtensionsTests
    {
        [Test]
        public void AddServiceBusMessageBus_DefaultConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddServiceBusMessageBus();

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }

        [Test]
        public void AddServiceBusMessageBus_CustomConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddServiceBusMessageBus("CustomKey");

            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("CustomKey"));
        }

        [Test]
        public void AddOpenTelemetryForServiceBusMessageBus_BuildsProvider()
        {
            using var provider = Sdk.CreateTracerProviderBuilder()
                .AddOpenTelemetryForServiceBusMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryForServiceBusMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateTracerProviderBuilder();

            var result = builder.AddOpenTelemetryForServiceBusMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void AddOpenTelemetryForServiceBusMessageBus_DoesNotOverwriteHostServiceName()
        {
            // Regression test: this used to call .SetResourceBuilder(...) with the library's
            // own ActivitySource name, silently replacing whatever service.name the host
            // application had already configured.
            using var provider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("my-app"))
                .AddOpenTelemetryForServiceBusMessageBus()
                .Build();

            var resource = provider!.GetResource();
            var serviceName = resource.Attributes.First(a => a.Key == "service.name").Value;

            Assert.That(serviceName, Is.EqualTo("my-app"));
        }

        [Test]
        public void AddOpenTelemetryMetricsForServiceBusMessageBus_RegistersMeterAndBuildsProvider()
        {
            using var provider = Sdk.CreateMeterProviderBuilder()
                .AddOpenTelemetryMetricsForServiceBusMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryMetricsForServiceBusMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateMeterProviderBuilder();

            var result = builder.AddOpenTelemetryMetricsForServiceBusMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public async Task AddServiceBusMessageBusHealthCheck_RegistersCheckThatReflectsClientState()
        {
            var sbClient = new ServiceBusClient(
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==");
            var clientMock = new Mock<IServiceBusClient>();
            clientMock.SetupGet(c => c.Client).Returns(sbClient);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHealthChecks()
                .AddServiceBusMessageBusHealthCheck(_ => clientMock.Object);

            using var provider = services.BuildServiceProvider();
            var healthCheckService = provider.GetRequiredService<HealthCheckService>();

            var report = await healthCheckService.CheckHealthAsync();

            Assert.That(report.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(report.Entries.ContainsKey("servicebus-messagebus"), Is.True);

            await sbClient.DisposeAsync();
        }
    }
}
