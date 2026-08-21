using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Consumers
{
    [TestFixture]
    public class RabbitMQDefaultStreamConsumerTests
    {
        [Test]
        public async Task StartAsync_RegistersHandlerOnStreamClient()
        {
            var clientMock = new Mock<IMessageBusStreamAsyncClient>();
            clientMock.SetupGet(c => c.EntityPath).Returns("stream-1");
            clientMock.Setup(c => c.RegisterHandlerAsync(
                    It.IsAny<Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>>>(),
                    It.IsAny<Func<Exception, CancellationToken, Task>>(),
                    It.IsAny<Func<CancellationToken, Task>?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handlerMock = new Mock<IMessageBusMessageHandler>();

            var sut = new RabbitMQDefaultStreamConsumer<IMessageBusMessageHandler>(clientMock.Object, handlerMock.Object, NullLogger<DefaultConsumer<IMessageBusMessageHandler>>.Instance);

            await sut.StartAsync(CancellationToken.None);

            clientMock.Verify(c => c.RegisterHandlerAsync(
                It.IsAny<Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>>>(),
                It.IsAny<Func<Exception, CancellationToken, Task>>(),
                It.IsAny<Func<CancellationToken, Task>?>(),
                false,
                CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task StopAsync_ClosesStreamClient()
        {
            var clientMock = new Mock<IMessageBusStreamAsyncClient>();
            clientMock.Setup(c => c.CloseAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var handlerMock = new Mock<IMessageBusMessageHandler>();

            var sut = new RabbitMQDefaultStreamConsumer<IMessageBusMessageHandler>(clientMock.Object, handlerMock.Object, NullLogger<DefaultConsumer<IMessageBusMessageHandler>>.Instance);

            await sut.StopAsync(CancellationToken.None);

            clientMock.Verify(c => c.CloseAsync(CancellationToken.None), Times.Once);
        }
    }
}
