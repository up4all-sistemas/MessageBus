using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Options;
using Up4All.Framework.MessageBus.Kafka.Pipelines;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Pipelines
{
    [TestFixture]
    public class KafkaMessageBusPipelineTests
    {
        private static KafkaMessageBusPipeline CreatePipeline(out ServiceCollection services)
        {
            services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var pipeline = new KafkaMessageBusPipeline(services, "MessageBusOptions");

            // AddOptions() (called by the pipeline constructor) registers the framework's own
            // IOptions<T> machinery, which would shadow a plain AddSingleton<IOptions<T>>
            // registered *before* it - so it's added after, to make sure it's the one resolved.
            services.AddSingleton<Microsoft.Extensions.Options.IOptions<KafkaMessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(new KafkaMessageBusOptions
            {
                ConnectionString = "localhost:1",
                TopicName = "topic-1",
                SubscriptionName = "sub-1"
            }));

            return pipeline;
        }

        [Test]
        public void Constructor_ExposesSubscriptionsAndProducersSubPipelines()
        {
            var pipeline = CreatePipeline(out var services);

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

            pipeline.Producers.AddPublisher("localhost:1", "topic-2");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>();

            Assert.That(publisher.TopicName, Is.EqualTo("topic-2"));
        }

        [Test]
        public void AddPublisher_Keyed_RegistersKeyedPublisher()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Producers.AddPublisher((object)"key-1", "localhost:1", "topic-2");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredKeyedService<IMessageBusPublisherAsync>("key-1");

            Assert.That(publisher, Is.Not.Null);
        }

        [Test]
        public void AddPublisherGeneric_ConnectionStringBased_RegistersPublisher()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Producers.AddPublisher<string>("localhost:1", "topic-2");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>();

            Assert.That(publisher.TopicName, Is.EqualTo("topic-2"));
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

            pipeline.Subscriptions.ListenSubscription("localhost:1", "topic-2", "sub-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("sub-2"));
        }

        [Test]
        public void ListenSubscription_Keyed_RegistersKeyedConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscription((object)"key-1", "localhost:1", "topic-2", "sub-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredKeyedService<IMessageBusAsyncConsumer>("key-1");

            Assert.That(consumer, Is.Not.Null);
        }

        [Test]
        public void ListenSubscriptionWithStructKey_OptionsBased_RegistersConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscriptionWithStructKey<int>();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("sub-1"));
        }

        [Test]
        public void ListenSubscriptionWithStructKey_ConnectionStringBased_RegistersConsumer()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscriptionWithStructKey<int>("localhost:1", "topic-2", "sub-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>();

            Assert.That(consumer.EntityPath, Is.EqualTo("sub-2"));
        }

        [Test]
        public void AddHandler_RegistersHostedService()
        {
            var pipeline = CreatePipeline(out var services);

            pipeline.Subscriptions.ListenSubscription().AddHandler(_ => new NoopHandler());

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.Any(), Is.True);
        }
    }

    [TestFixture]
    public class IoCExtensionsPipelineTests
    {
        [Test]
        public void AddKafkaMessageBus_ReturnsConfiguredPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddKafkaMessageBus("MyBus");

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
