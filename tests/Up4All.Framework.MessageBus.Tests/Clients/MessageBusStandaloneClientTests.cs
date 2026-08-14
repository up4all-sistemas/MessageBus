using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Clients
{
    [TestFixture]
    public class MessageBusStandaloneQueueClientTests
    {
        [Test]
        public void Constructor_SetsExposedProperties()
        {
            var client = new FakeStandaloneQueueClient("conn", "queue-1", 3);

            Assert.That(client.ExposedConnectionString, Is.EqualTo("conn"));
            Assert.That(client.EntityPath, Is.EqualTo("queue-1"));
            Assert.That(client.ExposedConnectionAttempts, Is.EqualTo(3));
            Assert.That(client.TopicName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Constructor_DefaultConnectionAttempts_IsEight()
        {
            var client = new FakeStandaloneQueueClient("conn", "queue-1");

            Assert.That(client.ExposedConnectionAttempts, Is.EqualTo(8));
        }

        [Test]
        public void Dispose_InvokesDisposePattern()
        {
            var client = new FakeStandaloneQueueClient("conn", "queue-1");

            client.Dispose();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }

        [Test]
        public async Task DisposeAsync_NoOverride_FallsBackToSynchronousDispose()
        {
            // Fake doesn't override DisposeAsyncCore(), so the base class default
            // (Dispose(true)) should run.
            var client = new FakeStandaloneQueueClient("conn", "queue-1");

            await client.DisposeAsync();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }
    }

    [TestFixture]
    public class MessageBusStandaloneStreamClientTests
    {
        [Test]
        public void Constructor_SetsExposedProperties()
        {
            var client = new FakeStandaloneStreamClient("conn", "stream-1", offset: 5, connectionAttempts: 4);

            Assert.That(client.ExposedConnectionString, Is.EqualTo("conn"));
            Assert.That(client.EntityPath, Is.EqualTo("stream-1"));
            Assert.That(client.ExposedConnectionAttempts, Is.EqualTo(4));
            Assert.That(client.ExposedOffset, Is.EqualTo(5));
            Assert.That(client.TopicName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Dispose_InvokesDisposePattern()
        {
            var client = new FakeStandaloneStreamClient("conn", "stream-1", offset: "first");

            client.Dispose();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }

        [Test]
        public async Task DisposeAsync_NoOverride_FallsBackToSynchronousDispose()
        {
            var client = new FakeStandaloneStreamClient("conn", "stream-1", offset: "first");

            await client.DisposeAsync();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }
    }

    [TestFixture]
    public class MessageBusStandaloneTopicClientTests
    {
        [Test]
        public void Constructor_SetsExposedProperties()
        {
            var client = new FakeStandaloneTopicClient("conn", "topic-1", 2);

            Assert.That(client.ExposedConnectionString, Is.EqualTo("conn"));
            Assert.That(client.TopicName, Is.EqualTo("topic-1"));
            Assert.That(client.ExposedConnectionAttempts, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_InvokesDisposePattern()
        {
            var client = new FakeStandaloneTopicClient("conn", "topic-1");

            client.Dispose();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }

        [Test]
        public async Task DisposeAsync_NoOverride_FallsBackToSynchronousDispose()
        {
            var client = new FakeStandaloneTopicClient("conn", "topic-1");

            await client.DisposeAsync();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }
    }

    [TestFixture]
    public class MessageBusStandaloneSubscriptonClientTests
    {
        [Test]
        public void Constructor_SetsExposedProperties()
        {
            var client = new FakeStandaloneSubscriptonClient("conn", "topic-1", "sub-1");

            Assert.That(client.ExposedConnectionString, Is.EqualTo("conn"));
            Assert.That(client.ExposedTopicName, Is.EqualTo("topic-1"));
            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));
        }

        [Test]
        public void Dispose_InvokesDisposePattern()
        {
            var client = new FakeStandaloneSubscriptonClient("conn", "topic-1", "sub-1");

            client.Dispose();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }

        [Test]
        public async Task DisposeAsync_NoOverride_FallsBackToSynchronousDispose()
        {
            var client = new FakeStandaloneSubscriptonClient("conn", "topic-1", "sub-1");

            await client.DisposeAsync();

            Assert.That(client.DisposeCalled, Is.True);
            Assert.That(client.LastDisposing, Is.True);
        }
    }
}
