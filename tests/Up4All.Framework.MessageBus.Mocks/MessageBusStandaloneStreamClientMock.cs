using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneStreamClientMock : MessageBusClientBaseMock, IMessageBusStandaloneStreamClient
    {
        protected MessageBusStandaloneStreamClientMock() : base()
        {
        }
    }

    public abstract class MessageBusStandaloneStreamAsyncClientMock : MessageBusAsyncClientBaseMock, IMessageBusStandaloneStreamAsyncClient
    {
        protected MessageBusStandaloneStreamAsyncClientMock() : base()
        {
        }
    }
}
