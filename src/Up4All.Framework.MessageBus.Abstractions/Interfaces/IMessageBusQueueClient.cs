namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusQueueClient : IMessageBusPublisher, IMessageBusConsumer
    {
    }

    public interface IMessageBusQueueAsyncClient : IMessageBusPublisherAsync, IMessageBusAsyncConsumer
    {
    }
}
