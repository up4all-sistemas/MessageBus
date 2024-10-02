namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusStandaloneStreamClient : IMessageBusPublisher, IMessageBusConsumer
    {
    }

    public interface IMessageBusStandaloneStreamAsyncClient : IMessageBusPublisherAsync, IMessageBusAsyncConsumer
    {
    }
}
