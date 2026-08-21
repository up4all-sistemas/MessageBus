using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Consumers
{
    [TestFixture]
    public class DefaultConsumerTests
    {
        [Test]
        public async Task StartAsync_RegistersHandlerWithAutoCompleteFalse()
        {
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);
            var token = new CancellationTokenSource().Token;

            await sut.StartAsync(token);

            Assert.That(consumer.CapturedHandler, Is.Not.Null);
            Assert.That(consumer.AutoCompleteReceived, Is.False);
            Assert.That(consumer.RegisterCancellationToken, Is.EqualTo(token));
        }

        [Test]
        public async Task RegisteredHandler_OnSuccess_ReturnsCompletedAndInvokesHandler()
        {
            var consumer = new FakeAsyncConsumer { EntityPath = "my-entity" };
            var handler = new FakeMessageHandler();
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);
            await sut.StartAsync(CancellationToken.None);

            var message = new ReceivedMessage();
            message.AddBody("payload");

            var status = await consumer.CapturedHandler!.Invoke(message, CancellationToken.None);

            Assert.That(status, Is.EqualTo(MessageReceivedStatus.Completed));
            Assert.That(handler.ReceivedCalls, Has.Count.EqualTo(1));
            Assert.That(handler.ReceivedCalls[0].EntityPath, Is.EqualTo("my-entity"));
            Assert.That(handler.ReceivedCalls[0].Message, Is.SameAs(message));
        }

        [Test]
        public async Task RegisteredHandler_OnException_ReturnsAbandonedAndInvokesErrorHandler()
        {
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler { ThrowOnReceive = true };
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);
            await sut.StartAsync(CancellationToken.None);

            var message = new ReceivedMessage();

            var status = await consumer.CapturedHandler!.Invoke(message, CancellationToken.None);

            Assert.That(status, Is.EqualTo(MessageReceivedStatus.Abandoned));
            Assert.That(handler.Errors, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task StopAsync_ClosesConsumer()
        {
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);
            var token = new CancellationTokenSource().Token;

            await sut.StopAsync(token);

            Assert.That(consumer.CloseCalled, Is.True);
            Assert.That(consumer.CloseCancellationToken, Is.EqualTo(token));
        }

        [Test]
        public void Dispose_ClosesConsumer()
        {
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);

            sut.Dispose();

            Assert.That(consumer.CloseCalled, Is.True);
            Assert.That(consumer.CloseCancellationToken, Is.EqualTo(CancellationToken.None));
        }

        [Test]
        public async Task DisposeAsync_ClosesConsumer()
        {
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            var sut = new DefaultConsumer<FakeMessageHandler>(consumer, handler, NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);

            await sut.DisposeAsync();

            Assert.That(consumer.CloseCalled, Is.True);
            Assert.That(consumer.CloseCancellationToken, Is.EqualTo(CancellationToken.None));
        }

        [Test]
        public async Task KeyedConstructor_ResolvesConsumerAndHandlerForGivenServiceKey()
        {
            const string serviceKey = "my-key";
            var consumer = new FakeAsyncConsumer();
            var handler = new FakeMessageHandler();
            var otherConsumer = new FakeAsyncConsumer();

            var services = new ServiceCollection();
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, consumer);
            services.AddKeyedSingleton<IMessageBusMessageHandler>(serviceKey, handler);
            services.AddKeyedSingleton<IMessageBusAsyncConsumer>("other-key", otherConsumer);
            services.AddKeyedSingleton<IMessageBusMessageHandler>("other-key", new FakeMessageHandler());
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger<DefaultConsumer<FakeMessageHandler>>>(
                NullLogger<DefaultConsumer<FakeMessageHandler>>.Instance);
            var provider = services.BuildServiceProvider();

            var sut = new DefaultConsumer<FakeMessageHandler>(serviceKey, provider);
            await sut.StartAsync(CancellationToken.None);

            Assert.That(consumer.CapturedHandler, Is.Not.Null);
            Assert.That(otherConsumer.CapturedHandler, Is.Null);
        }
    }
}
