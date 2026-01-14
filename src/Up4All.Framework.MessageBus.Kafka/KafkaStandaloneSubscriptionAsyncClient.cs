using Confluent.Kafka;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaStandaloneSubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName) 
        : KafkaStandaloneGenericSubscriptionAsyncClient<string>(connectionString, topicName, subscriptionName)
        , IMessageBusStandaloneAsyncConsumer
    {        
    }
}
