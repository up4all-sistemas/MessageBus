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
    public class RabbitMQStandaloneStreamAsyncClientTests
    {
        private static (RabbitMQStandaloneStreamAsyncClient client, Mock<IChannel> channel, Mock<IConnection> connection) CreateClient(
            StreamDeclareOptions? declareOpts = null)
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var connectionMock = RabbitMQMocks.CreateConnection(channelMock.Object);

            var client = new RabbitMQStandaloneStreamAsyncClient(RabbitMQMocks.Logger<RabbitMQStandaloneStreamAsyncClient>()
                , "amqp://localhost", "stream-1", offset: "first", persistent: true, connectionAttempts: 3, declareOpts)
            {
                Connection = connectionMock.Object
            };

            return (client, channelMock, connectionMock);
        }

        [Test]
        public void Constructor_ExposesStreamNameAsEntityPath()
        {
            var (client, _, _) = CreateClient();

            Assert.That(client.EntityPath, Is.EqualTo("stream-1"));
        }

        [Test]
        public async Task RegisterHandlerAsync_PassesOffsetArgumentToConsume()
        {
            var (client, channel, _) = CreateClient();

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                , It.Is<System.Collections.Generic.IDictionary<string, object?>>(d => (string)d[Arguments.StreamOffsetKey]! == "first")
                , It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsyncGeneric_PassesOffsetArgumentToConsume()
        {
            var (client, channel, _) = CreateClient();

            await client.RegisterHandlerAsync<string>((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                , It.Is<System.Collections.Generic.IDictionary<string, object?>>(d => (string)d[Arguments.StreamOffsetKey]! == "first")
                , It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsync_WithDeclareOptions_DeclaresStreamQueue()
        {
            var options = RabbitMQConsts.DefaultStreamDeclareOptions;
            var (client, channel, _) = CreateClient(options);

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask);

            channel.Verify(c => c.QueueDeclareAsync("stream-1", options.Durable, options.Exclusive, options.AutoDelete
                , options.Args, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_PublishesMessage()
        {
            var (client, channel, _) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "stream-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_Enumerable_PublishesEachMessage()
        {
            var (client, channel, _) = CreateClient();
            var messages = new[] { new MessageBusMessage(), new MessageBusMessage() };
            foreach (var m in messages) m.AddBody("x");

            await client.SendAsync((System.Collections.Generic.IEnumerable<MessageBusMessage>)messages);

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "stream-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendAsync_Model_SerializesAndPublishes()
        {
            var (client, channel, _) = CreateClient();

            await client.SendAsync(new { Name = "test" });

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "stream-1", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendManyAsync_PublishesEachModel()
        {
            var (client, channel, _) = CreateClient();

            await client.SendManyAsync(new[] { new { Name = "a" }, new { Name = "b" } });

            channel.Verify(c => c.BasicPublishAsync(string.Empty, "stream-1", false, It.IsAny<BasicProperties>()
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
    }

    [TestFixture]
    public class RabbitMQStreamAsyncClientTests
    {
        [Test]
        public void Constructor_UsesOptionsForConnectionAndStreamName()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                StreamName = "stream-from-options",
                ConnectionAttempts = 2
            });

            var client = new RabbitMQStreamAsyncClient(RabbitMQMocks.Logger<RabbitMQStreamAsyncClient>(), options, offset: "first");
            GC.SuppressFinalize(client);

            Assert.That(client.EntityPath, Is.EqualTo("stream-from-options"));
        }
    }
}
