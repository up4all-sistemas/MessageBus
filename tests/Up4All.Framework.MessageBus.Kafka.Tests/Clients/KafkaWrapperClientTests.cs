using Confluent.Kafka;

using Moq;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Kafka.Options;
using Up4All.Framework.MessageBus.Kafka.Tests.Support;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Clients
{
    [TestFixture]
    public class KafkaStandaloneWithStructKeyTopicAsyncClientTests
    {
        [Test]
        public async Task SendAsync_ProducesMessageUsingStructKey()
        {
            var client = new KafkaStandaloneWithStructKeyTopicAsyncClient<int>("localhost:1", "topic-1", connectionAttempts: 1);
            var producerMock = new Mock<IProducer<int, byte[]>>();
            producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<int, byte[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<int, byte[]>());
            KafkaTestHelpers.ReplaceProducer(client, producerMock.Object);

            var message = new Abstractions.Messages.MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageIdFromStruct(7);

            await client.SendAsync(message);

            producerMock.Verify(p => p.ProduceAsync("topic-1", It.Is<Message<int, byte[]>>(m => m.Key == 7), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Dispose_DisposesProducer()
        {
            var client = new KafkaStandaloneWithStructKeyTopicAsyncClient<int>("localhost:1", "topic-1", connectionAttempts: 1);
            var producerMock = new Mock<IProducer<int, byte[]>>();
            KafkaTestHelpers.ReplaceProducer(client, producerMock.Object);

            client.Dispose();

            producerMock.Verify(p => p.Dispose(), Times.Once);
        }
    }

    [TestFixture]
    public class KafkaOptionsBasedWrapperClientTests
    {
        private static Microsoft.Extensions.Options.IOptions<KafkaMessageBusOptions> CreateOptions() =>
            Microsoft.Extensions.Options.Options.Create(new KafkaMessageBusOptions
            {
                ConnectionString = "localhost:1",
                TopicName = "topic-1",
                SubscriptionName = "sub-1"
            });

        [Test]
        public void KafkaTopicAsyncClient_UsesOptionsForTopicName()
        {
            var client = new KafkaTopicAsyncClient(CreateOptions());

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));

            client.Dispose();
        }

        [Test]
        public void KafkaGenericTopicAsyncClient_UsesOptionsForTopicName()
        {
            var client = new KafkaGenericTopicAsyncClient<string>(CreateOptions());

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));

            client.Dispose();
        }

        [Test]
        public void KafkaWithStructKeyTopicAsyncClient_UsesOptionsForTopicName()
        {
            var client = new KafkaWithStructKeyTopicAsyncClient<int>(CreateOptions());

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));

            client.Dispose();
        }

        [Test]
        public void KafkaSubscriptionAsyncClient_UsesOptionsForSubscriptionName()
        {
            var client = new KafkaSubscriptionAsyncClient(CreateOptions());

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));

            client.Dispose();
        }

        [Test]
        public void KafkaGenericSubscriptionAsyncClient_UsesOptionsForSubscriptionName()
        {
            var client = new KafkaGenericSubscriptionAsyncClient<string>(CreateOptions());

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));

            client.Dispose();
        }

        [Test]
        public void KafkaWithStructKeySubscriptionAsyncClient_UsesOptionsForSubscriptionName()
        {
            var client = new KafkaWithStructKeySubscriptionAsyncClient<int>(CreateOptions());

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));

            client.Dispose();
        }
    }

    [TestFixture]
    public class KafkaStandaloneSubscriptionAsyncClientTests
    {
        [Test]
        public void Constructor_ExposesSubscriptionNameAsEntityPath()
        {
            var client = new KafkaStandaloneSubscriptionAsyncClient("localhost:1", "topic-1", "sub-1");

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));

            client.Dispose();
        }
    }
}
