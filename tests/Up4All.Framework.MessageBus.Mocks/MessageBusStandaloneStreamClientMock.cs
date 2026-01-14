using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneStreamAsyncClientMock : MessageBusAsyncClientBaseMock, IMessageBusStandaloneStreamAsyncClient
    {
        protected MessageBusStandaloneStreamAsyncClientMock() : base()
        {
        }
    }
}
