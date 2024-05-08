
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneTopicClient : MessageBusStandaloneClientBase, IMessageBusStandalonePublisher
    {
        protected string ConnectionString { get; private set; }
        protected string TopicName { get; private set; }

        public MessageBusStandaloneTopicClient(string connectionString, string topicName)
        {
            ConnectionString = connectionString;
            TopicName = topicName;
        }

        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
    }
}
