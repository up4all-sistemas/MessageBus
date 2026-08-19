using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Options;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Pipelines
{
    [TestFixture]
    public class MessageBusPipelineTests
    {
        [Test]
        public void Constructor_SetsServicesAndConfigurationBindKey()
        {
            var services = new ServiceCollection();

            var pipeline = new FakePipeline(services, "MyBus");

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MyBus"));
            Assert.That(pipeline.OptionsBuilder, Is.Null);
        }

        [Test]
        public void AddOptions_SetsOptionsBuilder()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");

            pipeline.CallAddOptions();

            Assert.That(pipeline.OptionsBuilder, Is.Not.Null);
        }

        [Test]
        public void AddOptions_BindsConfigurationSection()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("MyBus:ConnectionString", "amqp://localhost"),
                    new("MyBus:QueueName", "queue-1")
                ])
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            var pipeline = new FakePipeline(services, "MyBus");
            pipeline.CallAddOptions();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;

            Assert.That(options.ConnectionString, Is.EqualTo("amqp://localhost"));
            Assert.That(options.QueueName, Is.EqualTo("queue-1"));
        }

        [Test]
        public void Validate_InvokesValidateOnEachRegisteredInnerPipeline()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var inner1 = new FakeInnerPipelineBuilder();
            var inner2 = new FakeInnerPipelineBuilder();
            pipeline.AddInnerPipeline(inner1);
            pipeline.AddInnerPipeline(inner2);

            pipeline.Validate();

            Assert.That(inner1.ValidateCalled, Is.True);
            Assert.That(inner2.ValidateCalled, Is.True);
        }
    }

    [TestFixture]
    public class MessageBusFlowPipelineTests
    {
        [Test]
        public void Then_ReturnsMainPipeline()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            var result = publisher.Then();

            Assert.That(result, Is.SameAs(pipeline));
        }
    }

    [TestFixture]
    public class MessageBusPublisherPipelineTests
    {
        [Test]
        public void Validate_WhenPublisherNotDefined_Throws()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            Assert.Throws<ArgumentException>(() => publisher.Validate());
        }

        [Test]
        public void Validate_WhenPublisherDefined_DoesNotThrow()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            publisher.MarkPublisherDefined();

            Assert.DoesNotThrow(() => publisher.Validate());
        }
    }

    [TestFixture]
    public class MessageBusConsumerPipelineTests
    {
        [Test]
        public void Validate_WhenHandlerNotDefined_Throws()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            Assert.Throws<ArgumentException>(() => consumerPipeline.Validate());
        }

        [Test]
        public void AddHandler_RegistersHandlerAndMarksDefined()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddHandler<FakeMessageHandler>();

            var provider = services.BuildServiceProvider();
            var handler = provider.GetService<IMessageBusMessageHandler>();

            Assert.That(handler, Is.InstanceOf<FakeMessageHandler>());
            Assert.DoesNotThrow(() => consumerPipeline.Validate());
        }

        [Test]
        public void AddHandler_WithFactory_RegistersHandlerAndMarksDefined()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);
            var instance = new FakeMessageHandler();

            consumerPipeline.AddHandler(_ => instance);

            var provider = services.BuildServiceProvider();
            var handler = provider.GetService<IMessageBusMessageHandler>();

            Assert.That(handler, Is.SameAs(instance));
        }

        [Test]
        public void AddKeyedHandler_RegistersKeyedHandler()
        {
            var services = new ServiceCollection();
            const string serviceKey = "my-key";
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            var result = consumerPipeline.AddKeyedHandler<FakeMessageHandler>(serviceKey);

            Assert.That(result, Is.SameAs(consumerPipeline));

            var provider = services.BuildServiceProvider();
            var handler = provider.GetKeyedService<IMessageBusMessageHandler>(serviceKey);

            Assert.That(handler, Is.InstanceOf<FakeMessageHandler>());
            Assert.That(provider.GetService<IMessageBusMessageHandler>(), Is.Null);
        }

        [Test]
        public void AddKeyedHandler_WithFactory_RegistersKeyedHandlerBuiltByFactory()
        {
            var services = new ServiceCollection();
            const string serviceKey = "my-key";
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);
            var instance = new FakeMessageHandler();

            var result = consumerPipeline.AddKeyedHandler<FakeMessageHandler>(serviceKey, (_, _) => instance);

            Assert.That(result, Is.SameAs(consumerPipeline));

            var provider = services.BuildServiceProvider();
            var handler = provider.GetKeyedService<IMessageBusMessageHandler>(serviceKey);

            Assert.That(handler, Is.SameAs(instance));
        }

        [Test]
        public void AddDefaultHostedService_RegistersHostedService()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddDefaultHostedService();

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.OfType<FakeHostedConsumer>().Any(), Is.True);
        }

        [Test]
        public void AddHostedService_RemovesPreviouslyRegisteredDefaultConsumer()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMessageDefaultConsumer, FakeHostedConsumer>();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddHostedService<FakeHostedConsumer>();

            var registeredCount = services.Count(sd => sd.ServiceType == typeof(IMessageDefaultConsumer));
            Assert.That(registeredCount, Is.EqualTo(0));
        }

        [Test]
        public void AddKeyedHostedService_RegistersHostedServiceBuiltWithServiceKeyAndProvider()
        {
            var services = new ServiceCollection();
            const string serviceKey = "my-key";
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, consumer);
            services.AddKeyedSingleton<IMessageBusMessageHandler>(serviceKey, handler);
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            var result = consumerPipeline.AddKeyedHostedService<DefaultKeyedConsumer>(serviceKey);

            Assert.That(result, Is.SameAs(consumerPipeline));

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            var keyedConsumer = hostedServices.OfType<DefaultKeyedConsumer>().SingleOrDefault();

            Assert.That(keyedConsumer, Is.Not.Null);
        }

        [Test]
        public async Task AddKeyedHostedService_HostedServiceResolvesDependenciesForGivenKey()
        {
            var services = new ServiceCollection();
            const string serviceKey = "my-key";
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, consumer);
            services.AddKeyedSingleton<IMessageBusMessageHandler>(serviceKey, handler);
            var otherConsumer = new FakeAsyncConsumer();
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>("other-key", otherConsumer);
            services.AddKeyedSingleton<IMessageBusMessageHandler>("other-key", new FakeMessageHandler());
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddKeyedHostedService<DefaultKeyedConsumer>(serviceKey);

            var provider = services.BuildServiceProvider();
            var keyedConsumer = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>()
                .OfType<DefaultKeyedConsumer>()
                .Single();

            await keyedConsumer.StartAsync(CancellationToken.None);

            Assert.That(consumer.CapturedHandler, Is.Not.Null);
            Assert.That(otherConsumer.CapturedHandler, Is.Null);
        }

        [Test]
        public void AddDefaultKeyedHostedService_RegistersDefaultKeyedConsumer()
        {
            var services = new ServiceCollection();
            const string serviceKey = "my-key";
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, new FakeAsyncConsumer());
            services.AddKeyedSingleton<IMessageBusMessageHandler>(serviceKey, new FakeMessageHandler());
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            var result = consumerPipeline.AddDefaultKeyedHostedService(serviceKey);

            Assert.That(result, Is.SameAs(consumerPipeline));

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            var keyedConsumer = hostedServices.OfType<DefaultKeyedConsumer>().SingleOrDefault();

            Assert.That(keyedConsumer, Is.Not.Null);
        }
    }
}
