using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusQueueClientMock : MessageBusStandaloneQueueClientMock, IMessageBusQueueClient
    {
        protected MessageBusQueueClientMock() : base()
        {
        }
    }
}
