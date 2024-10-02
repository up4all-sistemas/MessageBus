namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusStandaloneQueueClient : IMessageBusPublisher, IMessageBusConsumer
    {
    }

    public interface IMessageBusStandaloneQueueAsyncClient : IMessageBusPublisherAsync, IMessageBusAsyncConsumer
    {
    }
}
