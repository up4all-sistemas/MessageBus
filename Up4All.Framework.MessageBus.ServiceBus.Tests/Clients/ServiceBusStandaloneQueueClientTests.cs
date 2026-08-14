using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Clients
{
    [TestFixture]
    public class ServiceBusStandaloneQueueAsyncClientTests
    {
        private const string FakeConnectionString =
            "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==";

        [Test]
        public void Constructor_ExposesQueueNameAsEntityPath()
        {
            var client = new ServiceBusStandaloneQueueAsyncClient(NullLogger<ServiceBusStandaloneQueueAsyncClient>.Instance
                , FakeConnectionString, "queue-1", connectionAttemps: 1);

            Assert.That(client.EntityPath, Is.EqualTo("queue-1"));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneQueueAsyncClient(NullLogger<ServiceBusStandaloneQueueAsyncClient>.Instance
                , FakeConnectionString, "queue-1", connectionAttemps: 1);

            Assert.DoesNotThrow(() => client.Dispose());
        }

        [Test]
        public async Task DisposeAsync_DoesNotThrow()
        {
            var client = new ServiceBusStandaloneQueueAsyncClient(NullLogger<ServiceBusStandaloneQueueAsyncClient>.Instance
                , FakeConnectionString, "queue-1", connectionAttemps: 1);

            Assert.DoesNotThrowAsync(async () => await client.DisposeAsync());
        }

        [Test]
        public async Task CloseAsync_DoesNotThrowWhenNoProcessorWasEverCreated()
        {
            var client = new ServiceBusStandaloneQueueAsyncClient(NullLogger<ServiceBusStandaloneQueueAsyncClient>.Instance
                , FakeConnectionString, "queue-1", connectionAttemps: 1);

            Assert.DoesNotThrowAsync(async () => await client.CloseAsync());
        }
    }

    [TestFixture]
    public class ServiceBusQueueAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionAndQueueName()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new MessageBusOptions
            {
                ConnectionString = "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==",
                QueueName = "queue-from-options"
            });

            var client = new ServiceBusQueueAsyncClient(NullLogger<ServiceBusQueueAsyncClient>.Instance, options);

            Assert.That(client.EntityPath, Is.EqualTo("queue-from-options"));

            client.Dispose();
        }
    }
}
