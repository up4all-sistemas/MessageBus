using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaGenericSubscriptionAsyncClient<TMessageKey>(IOptions<KafkaMessageBusOptions> messageOptions)
        : KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName)
        , IMessageBusAsyncConsumer
        where TMessageKey : class
    {
    }

    public class KafkaWithStructKeySubscriptionAsyncClient<TMessageKey>(IOptions<KafkaMessageBusOptions> messageOptions)
        : KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName)
        , IMessageBusAsyncConsumer
        where TMessageKey : struct
    {
    }

    public class KafkaSubscriptionAsyncClient(IOptions<KafkaMessageBusOptions> messageOptions)
        : KafkaGenericSubscriptionAsyncClient<string>(messageOptions)
        , IMessageBusAsyncConsumer
    {
    }
}
