
using System.Collections.Generic;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneTopicClientMock : MessageBusClientBaseMock, IMessageBusStandalonePublisher
    {
        protected MessageBusStandaloneTopicClientMock() : base()
        {
        }

        public string TopicName => string.Empty;

        public abstract void Send(MessageBusMessage message);
        public abstract void Send(IEnumerable<MessageBusMessage> messages);
        public abstract void Send<TModel>(TModel model);
        public abstract void SendMany<TModel>(IEnumerable<TModel> models);
    }
}
