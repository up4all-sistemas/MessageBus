using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneTopicClientMock : MessageBusClientBaseMock, IMessageBusStandalonePublisher
    {
        protected MessageBusStandaloneTopicClientMock() : base()
        {
        }
    }
}
