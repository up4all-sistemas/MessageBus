using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaStandaloneSubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
        : KafkaStandaloneGenericSubscriptionAsyncClient<string>(connectionString, topicName, subscriptionName)
        , IMessageBusStandaloneAsyncConsumer
    {
    }
}
