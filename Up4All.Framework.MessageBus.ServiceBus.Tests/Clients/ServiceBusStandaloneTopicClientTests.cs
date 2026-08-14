using Microsoft.Extensions.Logging.Abstractions;

using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Clients
{
    [TestFixture]
    public class ServiceBusStandaloneTopicAsyncClientTests
    {
        private const string FakeConnectionString =
            "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==";

        [Test]
        public void Constructor_ExposesTopicName()
        {
            var client = new ServiceBusStandaloneTopicAsyncClient(NullLogger<ServiceBusStandaloneTopicAsyncClient>.Instance
                , FakeConnectionString, "topic-1", connectionAttempts: 1);

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneTopicAsyncClient(NullLogger<ServiceBusStandaloneTopicAsyncClient>.Instance
                , FakeConnectionString, "topic-1", connectionAttempts: 1);

            Assert.DoesNotThrow(() => client.Dispose());
        }

        [Test]
        public async Task DisposeAsync_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneTopicAsyncClient(NullLogger<ServiceBusStandaloneTopicAsyncClient>.Instance
                , FakeConnectionString, "topic-1", connectionAttempts: 1);

            Assert.DoesNotThrowAsync(async () => await client.DisposeAsync());
        }

        [Test]
        public async Task CloseAsync_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneTopicAsyncClient(NullLogger<ServiceBusStandaloneTopicAsyncClient>.Instance
                , FakeConnectionString, "topic-1", connectionAttempts: 1);

            Assert.DoesNotThrowAsync(async () => await client.CloseAsync());
        }
    }

    [TestFixture]
    public class ServiceBusTopicAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionAndTopicName()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new MessageBusOptions
            {
                ConnectionString = "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==",
                TopicName = "topic-from-options"
            });

            var client = new ServiceBusTopicAsyncClient(NullLogger<ServiceBusTopicAsyncClient>.Instance, options);

            Assert.That(client.TopicName, Is.EqualTo("topic-from-options"));

            client.Dispose();
        }
    }
}
