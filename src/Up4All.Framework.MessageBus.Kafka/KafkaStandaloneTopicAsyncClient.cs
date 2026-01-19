using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaStandaloneTopicAsyncClient(string connectionString, string topicName, int connectionAttempts = 8)
        : KafkaStandaloneGenericTopicAsyncClient<string>(connectionString, topicName, connectionAttempts), IMessageBusStandalonePublisherAsync
    {
        public new async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            message.SetMessageId(Guid.NewGuid().ToString());
            await base.SendAsync(message, cancellationToken);
        }
    }
}
