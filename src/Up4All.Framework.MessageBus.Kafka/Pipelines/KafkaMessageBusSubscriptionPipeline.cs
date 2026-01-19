using Microsoft.Extensions.DependencyInjection;

using System;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Pipelines
{
    public class KafkaMessageBusSubscriptionPipeline(KafkaMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<KafkaMessageBusPipeline, KafkaMessageBusOptions>(pipeline)
    {
        public KafkaMessageBusSubscriptionPipeline ListenSubscription()
        {            
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaSubscriptionAsyncClient>();            
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageKey>()
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaGenericSubscriptionAsyncClient<TMessageKey>>();            
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription(string connectionString, string topicName, string subscriptionName)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneSubscriptionAsyncClient(connectionString, topicName, subscriptionName));
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageKey>(string connectionString, string topicName, string subscriptionName)
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneWithGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));            
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscriptionWithStructKey<TMessageKey>()
            where TMessageKey : struct
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaWithStructKeySubscriptionAsyncClient<TMessageKey>>();            
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscriptionWithStructKey<TMessageKey>(string connectionString, string topicName, string subscriptionName)
            where TMessageKey : struct
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            return this;
        }

        public override IConsumerPipelineBuilder AddDefaultHostedService()
        {
            AddHostedService<DefaultConsumer>();
            return this;
        }
    }
}
