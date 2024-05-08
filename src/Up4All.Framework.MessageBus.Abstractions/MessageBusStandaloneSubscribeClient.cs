using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneSubscribeClient : MessageBusStandaloneClientBase, IMessageBusStandaloneConsumer
    {
        protected string ConnectionString { get; private set; }
        protected string TopicName { get; private set; }
        protected string SubscriptionName { get; private set; }

        public MessageBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName)
        {
            ConnectionString = connectionString;
            TopicName = topicName;
            SubscriptionName = subscriptionName;
        }

        public abstract void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task Close();

    }
}
