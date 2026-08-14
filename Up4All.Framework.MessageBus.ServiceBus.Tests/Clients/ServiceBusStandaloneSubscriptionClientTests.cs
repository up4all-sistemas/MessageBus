using Microsoft.Extensions.Logging.Abstractions;

using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Clients
{
    [TestFixture]
    public class ServiceBusStandaloneSubscriptionAsyncClientTests
    {
        private const string FakeConnectionString =
            "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==";

        [Test]
        public void Constructor_ExposesSubscriptionNameAsEntityPath()
        {
            var client = new ServiceBusStandaloneSubscriptionAsyncClient(NullLogger<ServiceBusStandaloneSubscriptionAsyncClient>.Instance
                , FakeConnectionString, "topic-1", "sub-1", connectionAttempts: 1);

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneSubscriptionAsyncClient(NullLogger<ServiceBusStandaloneSubscriptionAsyncClient>.Instance
                , FakeConnectionString, "topic-1", "sub-1", connectionAttempts: 1);

            Assert.DoesNotThrow(() => client.Dispose());
        }

        [Test]
        public async Task DisposeAsync_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneSubscriptionAsyncClient(NullLogger<ServiceBusStandaloneSubscriptionAsyncClient>.Instance
                , FakeConnectionString, "topic-1", "sub-1", connectionAttempts: 1);

            Assert.DoesNotThrowAsync(async () => await client.DisposeAsync());
        }

        [Test]
        public async Task CloseAsync_DoesNotThrowWhenNoProcessorWasEverCreated()
        {
            var client = new ServiceBusStandaloneSubscriptionAsyncClient(NullLogger<ServiceBusStandaloneSubscriptionAsyncClient>.Instance
                , FakeConnectionString, "topic-1", "sub-1", connectionAttempts: 1);

            Assert.DoesNotThrowAsync(async () => await client.CloseAsync());
        }
    }

    [TestFixture]
    public class ServiceBusSubscriptionAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionTopicAndSubscription()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new MessageBusOptions
            {
                ConnectionString = "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==",
                TopicName = "topic-from-options",
                SubscriptionName = "sub-from-options"
            });

            var client = new ServiceBusSubscriptionAsyncClient(NullLogger<ServiceBusSubscriptionAsyncClient>.Instance, options);

            Assert.That(client.EntityPath, Is.EqualTo("sub-from-options"));

            client.Dispose();
        }
    }
}
