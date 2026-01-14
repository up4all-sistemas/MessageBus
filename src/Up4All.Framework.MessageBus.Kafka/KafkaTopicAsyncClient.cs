using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaGenericTopicAsyncClient<TMessageKey>(IOptions<KafkaMessageBusOptions> messageOptions)
            : KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts)
        , IMessageBusPublisherAsync
        where TMessageKey : class
    {
    }

    public class KafkaWithStructKeyTopicAsyncClient<TMessageKey>(IOptions<KafkaMessageBusOptions> messageOptions)
            : KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts)
        , IMessageBusPublisherAsync
        where TMessageKey : struct
    {
    }

    public class KafkaTopicAsyncClient(IOptions<KafkaMessageBusOptions> messageOptions)
            : KafkaGenericTopicAsyncClient<string>(messageOptions)
        , IMessageBusPublisherAsync
    {        
    }
}
