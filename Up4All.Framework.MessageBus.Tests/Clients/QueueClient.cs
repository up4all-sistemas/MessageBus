using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Mocks;

namespace Up4All.Framework.MessageBus.Tests.Clients
{
    public class QueueClient : MessageBusStandaloneQueueClientMock
    {
        public override void Close()
        {
            //ignored
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action? onIdle = null, bool autoComplete = false)
        {
            //ignored
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action? onIdle = null, bool autoComplete = false)
        {
            //ignored
        }

        public override void Send(MessageBusMessage message)
        {
            //ignored
        }

        public override void Send(IEnumerable<MessageBusMessage> messages)
        {
            //ignored
        }

        public override void Send<TModel>(TModel model)
        {
            //ignored
        }

        public override void SendMany<TModel>(IEnumerable<TModel> models)
        {
            //ignored
        }
    }
}
