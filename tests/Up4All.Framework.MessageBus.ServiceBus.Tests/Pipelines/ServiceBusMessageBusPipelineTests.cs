using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.ServiceBus.Extensions;
using Up4All.Framework.MessageBus.ServiceBus.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Pipelines
{
    [TestFixture]
    public class ServiceBusMessageBusPipelineTests
    {
        private const string FakeConnectionString =
            "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==";

        private static ServiceBusMessageBusPipeline CreatePipeline(out ServiceCollection services)
        {
            services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var pipeline = new ServiceBusMessageBusPipeline(services, "MessageBusOptions");

            // AddOptions() (called by the pipeline constructor) registers the framework's own
            // IOptions<T> machinery, which would shadow a plain AddSingleton<IOptions<T>>
            // registered *before* it - so it's added after, to make sure it's the one resolved.
            services.AddSingleton<Microsoft.Extensions.Options.IOptions<MessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(new MessageBusOptions
            {
                ConnectionString = FakeConnectionString,
                QueueName = "queue-1",
                TopicName = "topic-1",
                SubscriptionName = "sub-1"
            }));

            return pipeline;
        }

        [Test]
        public void Constructor_ExposesQueuesSubscriptionsAndProducersSubPipelines()
        {
            var pipeline = CreatePipeline(out var services);

            Assert.That(pipeline.Queues, Is.Not.Null);
            Assert.That(pipeline.Subscriptions, Is.Not.Null);
            Assert.That(pipeline.Producers, Is.Not.Null);
            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }

        [Test]
        public void Validate_NothingConfigured_Throws()
        {
            var pipeline = CreatePipeline(out _);

            Assert.Throws<System.ArgumentException>(() => pipeline.Validate());
        }

        [Test]
        public void AddPublisher_OptionsBased_RegistersPublisherResolvableFromOptions()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Producers.AddPublisher();

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>();

            Assert.That(publisher.TopicName, Is.EqualTo("topic-1"));
        }

        [Test]
        public void AddPublisher_ConnectionStringBased_RegistersPublisher()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Producers.AddPublisher(FakeConnectionString, "topic-2");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>();

            Assert.That(publisher.TopicName, Is.EqualTo("topic-2"));
        }

        [Test]
        public void AddPublisher_Keyed_RegistersKeyedPublisher()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Producers.AddPublisher((object)"key-1", FakeConnectionString, "topic-2");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredKeyedService<IMessageBusPublisherAsync>("key-1");

            Assert.That(publisher, Is.Not.Null);
        }

        [Test]
        public void ListenQueue_OptionsBased_RegistersConsumerResolvableFromOptions()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Queues.ListenQueue();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("queue-1"));
        }

        [Test]
        public void ListenQueue_ConnectionStringBased_RegistersConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Queues.ListenQueue(FakeConnectionString, "queue-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("queue-2"));
        }

        [Test]
        public void ListenQueue_Keyed_RegistersKeyedConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Queues.ListenQueue((object)"key-1", FakeConnectionString, "queue-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredKeyedService<IMessageBusAsyncConsumer>("key-1");

            Assert.That(consumer, Is.Not.Null);
        }

        [Test]
        public void AddDefaultHostedService_OnQueuePipeline_RegistersHostedService()
        {
            var pipeline = CreatePipeline(out var services);
            pipeline.Queues.ListenQueue();
            pipeline.Queues.AddHandler(_ => new NoopHandler());

            pipeline.Queues.AddDefaultHostedService();

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.Any(), Is.True);
        }

        [Test]
        public void ListenSubscription_OptionsBased_RegistersConsumerResolvableFromOptions()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscription();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("sub-1"));
        }

        [Test]
        public void ListenSubscription_ConnectionStringBased_RegistersConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscription(FakeConnectionString, "topic-2", "sub-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("sub-2"));
        }

        [Test]
        public void ListenSubscription_Keyed_RegistersKeyedConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscription((object)"key-1", FakeConnectionString, "topic-2", "sub-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredKeyedService<IMessageBusAsyncConsumer>("key-1");

            Assert.That(consumer, Is.Not.Null);
        }

        [Test]
        public void AddDefaultHostedService_OnSubscriptionPipeline_RegistersHostedService()
        {
            var pipeline = CreatePipeline(out var services);
            pipeline.Subscriptions.ListenSubscription();
            pipeline.Subscriptions.AddHandler(_ => new NoopHandler());

            pipeline.Subscriptions.AddDefaultHostedService();

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.Any(), Is.True);
        }
    }

    [TestFixture]
    public class IoCExtensionsPipelineTests
    {
        [Test]
        public void AddServiceBusMessageBus_ReturnsConfiguredPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddServiceBusMessageBus("MyBus");

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MyBus"));
        }
    }

    internal class NoopHandler : Abstractions.Handlers.IMessageBusMessageHandler
    {
        public Task OnErrorAsync(System.Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task OnMessageReceivedAsync(string entityPath, Abstractions.Messages.ReceivedMessage message, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
