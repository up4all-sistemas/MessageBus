namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusStreamClient : IMessageBusPublisher, IMessageBusConsumer
    {
    }

    public interface IMessageBusStreamAsyncClient : IMessageBusPublisherAsync, IMessageBusAsyncConsumer
    {
    }
}
