using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneQueueClientMock : MessageBusClientBaseMock, IMessageBusStandaloneQueueClient
    {
        protected MessageBusStandaloneQueueClientMock() : base()
        {
        }
    }

    public abstract class MessageBusStandaloneQueueAsyncClientMock : MessageBusAsyncClientBaseMock, IMessageBusStandaloneQueueAsyncClient
    {
        protected MessageBusStandaloneQueueAsyncClientMock() : base()
        {
        }
    }
}
