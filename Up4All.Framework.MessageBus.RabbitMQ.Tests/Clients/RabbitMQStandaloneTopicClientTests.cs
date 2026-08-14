using Microsoft.Extensions.Options;

using Moq;

using RabbitMQ.Client;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using RabbitMQExchangeType = Up4All.Framework.MessageBus.RabbitMQ.Consts.ExchangeType;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Tests.Support;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Clients
{
    [TestFixture]
    public class RabbitMQStandaloneTopicAsyncClientTests
    {
        private static (RabbitMQStandaloneTopicAsyncClient client, Mock<IChannel> channel, Mock<IConnection> connection) CreateClient(
            ExchangeDeclareOptions? declareOpts = null)
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var connectionMock = RabbitMQMocks.CreateConnection(channelMock.Object);

            var client = new RabbitMQStandaloneTopicAsyncClient(RabbitMQMocks.Logger<RabbitMQStandaloneTopicAsyncClient>()
                , "amqp://localhost", "topic-1", persistent: true, connectionAttemps: 3, declareOpts: declareOpts)
            {
                Connection = connectionMock.Object
            };

            return (client, channelMock, connectionMock);
        }

        [Test]
        public void Constructor_ExposesTopicName()
        {
            var (client, _, _) = CreateClient();

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));
        }

        [Test]
        public async Task SendAsync_PublishesUsingTopicAsExchange()
        {
            var (client, channel, _) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            channel.Verify(c => c.BasicPublishAsync("topic-1", string.Empty, false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_WithoutDeclareOptions_DoesNotDeclareExchange()
        {
            var (client, channel, _) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            channel.Verify(c => c.ExchangeDeclareAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                , It.IsAny<System.Collections.Generic.IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendAsync_WithDeclareOptions_DeclaresExchange()
        {
            var options = RabbitMQConsts.DefaultExchangeDeclareOptions;
            var (client, channel, _) = CreateClient(options);
            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            channel.Verify(c => c.ExchangeDeclareAsync("topic-1", RabbitMQExchangeType.Topic, options.Durable, options.AutoDelete
                , options.Args, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_Enumerable_PublishesEachMessage()
        {
            var (client, channel, _) = CreateClient();
            var messages = new[] { new MessageBusMessage(), new MessageBusMessage() };
            foreach (var m in messages) m.AddBody("x");

            await client.SendAsync((System.Collections.Generic.IEnumerable<MessageBusMessage>)messages);

            channel.Verify(c => c.BasicPublishAsync("topic-1", string.Empty, false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendAsync_Model_SerializesAndPublishes()
        {
            var (client, channel, _) = CreateClient();

            await client.SendAsync(new { Name = "test" });

            channel.Verify(c => c.BasicPublishAsync("topic-1", string.Empty, false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendManyAsync_PublishesEachModel()
        {
            var (client, channel, _) = CreateClient();

            await client.SendManyAsync(new[] { new { Name = "a" }, new { Name = "b" } });

            channel.Verify(c => c.BasicPublishAsync("topic-1", string.Empty, false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public void Dispose_ClosesConnection()
        {
            var (client, channel, connection) = CreateClient();

            Assert.DoesNotThrow(() => client.Dispose());

            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CloseAsync_WithoutSendFirst_ClosesConnectionOnlyAndDoesNotThrow()
        {
            // Channel is never assigned unless a Send*/InitializeAsync happened, so
            // CloseAsync() must tolerate a null Channel here.
            var (client, channel, connection) = CreateClient();

            await client.CloseAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CloseAsync_AfterSend_ClosesChannelAndConnection()
        {
            var (client, channel, connection) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");
            await client.SendAsync(message);

            await client.CloseAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DisposeAsync_AfterSend_ClosesChannelAndConnection()
        {
            var (client, channel, connection) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");
            await client.SendAsync(message);

            await client.DisposeAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [TestFixture]
    public class RabbitMQTopicAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionAndTopicName()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                TopicName = "topic-from-options",
                ConnectionAttempts = 2
            });

            var client = new RabbitMQTopicAsyncClient(RabbitMQMocks.Logger<RabbitMQTopicAsyncClient>(), options);
            GC.SuppressFinalize(client);

            Assert.That(client.TopicName, Is.EqualTo("topic-from-options"));
        }
    }
}
