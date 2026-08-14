using Microsoft.Extensions.Options;

using Moq;

using RabbitMQ.Client;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Tests.Support;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Clients
{
    [TestFixture]
    public class RabbitMQStandaloneQueueAsyncClientTests
    {
        private static (RabbitMQStandaloneQueueAsyncClient client, Mock<IChannel> channel, Mock<IConnection> connection) CreateClient(
            QueueDeclareOptions? declareOpts = null)
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var connectionMock = RabbitMQMocks.CreateConnection(channelMock.Object);

            var client = new RabbitMQStandaloneQueueAsyncClient(RabbitMQMocks.Logger<RabbitMQStandaloneQueueAsyncClient>()
                , "amqp://localhost", "queue-1", persistent: true, connectionAttempts: 3, declareOpts)
            {
                Connection = connectionMock.Object
            };

            return (client, channelMock, connectionMock);
        }

        [Test]
        public void Constructor_ExposesQueueNameAsEntityPath()
        {
            var (client, _, _) = CreateClient();

            Assert.That(client.EntityPath, Is.EqualTo("queue-1"));
        }

        [Test]
        public async Task RegisterHandlerAsync_ConfiguresQosAndConsume()
        {
            var (client, channel, connection) = CreateClient();

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.BasicQosAsync(0, 1, false, It.IsAny<CancellationToken>()), Times.Once);
            channel.Verify(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                , It.IsAny<System.Collections.Generic.IDictionary<string, object?>>(), It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()), Times.Once);
            connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsync_CalledTwice_ReusesSameChannel()
        {
            var (client, _, connection) = CreateClient();

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);
            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsync_WithDeclareOptions_DeclaresQueue()
        {
            var options = RabbitMQConsts.DefaultQueueDeclareOptions;
            var (client, channel, _) = CreateClient(options);

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.QueueDeclareAsync("queue-1", options.Durable, options.Exclusive, options.AutoDelete
                , options.Args, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsyncGeneric_ConfiguresQosAndConsume()
        {
            var (client, channel, _) = CreateClient();

            await client.RegisterHandlerAsync<string>((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.BasicQosAsync(0, 1, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_PublishesMessage()
        {
            var (client, channel, _) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "queue-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_Enumerable_PublishesEachMessage()
        {
            var (client, channel, _) = CreateClient();
            var messages = new[] { new MessageBusMessage(), new MessageBusMessage() };
            foreach (var m in messages) m.AddBody("x");

            await client.SendAsync((System.Collections.Generic.IEnumerable<MessageBusMessage>)messages);

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "queue-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendAsync_Model_SerializesAndPublishes()
        {
            var (client, channel, _) = CreateClient();

            await client.SendAsync(new { Name = "test" });

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "queue-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendManyAsync_PublishesEachModel()
        {
            var (client, channel, _) = CreateClient();

            await client.SendManyAsync(new[] { new { Name = "a" }, new { Name = "b" } });

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "queue-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task CloseAsync_ClosesChannelAndConnection()
        {
            var (client, channel, connection) = CreateClient();
            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            await client.CloseAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Dispose_ClosesConnectionSynchronously()
        {
            var (client, _, connection) = CreateClient();

            Assert.DoesNotThrow(() => client.Dispose());

            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DisposeAsync_ClosesChannelAndConnection()
        {
            var (client, channel, connection) = CreateClient();
            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            await client.DisposeAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DisposeAsync_WithoutRegisterHandlerFirst_ClosesConnectionOnly()
        {
            var (client, channel, connection) = CreateClient();

            await client.DisposeAsync();

            channel.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            connection.Verify(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [TestFixture]
    public class RabbitMQQueueAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionAndQueueName()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                QueueName = "queue-from-options",
                PersistentMessages = false,
                ConnectionAttempts = 2
            });

            var client = new RabbitMQQueueAsyncClient(RabbitMQMocks.Logger<RabbitMQQueueAsyncClient>(), options);
            // Connection/Channel are never assigned here (no broker call was made), so avoid
            // running the class's finalizer - which calls Connection.CloseAsync() - on GC.
            GC.SuppressFinalize(client);

            Assert.That(client.EntityPath, Is.EqualTo("queue-from-options"));
        }
    }
}
