using Microsoft.Extensions.DependencyInjection;

using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Pipelines
{
    public class KafkaMessageBusPublisherPipeline(KafkaMessageBusPipeline pipeline)
        : MessageBusPublisherPipeline<KafkaMessageBusPipeline, KafkaMessageBusOptions>(pipeline)
    {

        public KafkaMessageBusPublisherPipeline AddPublisher()
        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync, KafkaTopicAsyncClient>();
            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageKey>()
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync, KafkaGenericTopicAsyncClient<TMessageKey>>();
            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher(string connectionString, string topicName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp => new KafkaStandaloneTopicAsyncClient(connectionString, topicName, connectionAttempts));
            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher(object serviceKey, string connectionString, string topicName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusPublisherAsync>(serviceKey, (sp, key) => new KafkaStandaloneTopicAsyncClient(connectionString, topicName, connectionAttempts));
            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageKey>(string connectionString, string topicName
            , int connectionAttempts = 8)
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp => new KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(connectionString, topicName, connectionAttempts));
            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageKey>(object serviceKey, string connectionString, string topicName
            , int connectionAttempts = 8)
            where TMessageKey : class
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusPublisherAsync>(serviceKey, (sp, key) => new KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(connectionString, topicName, connectionAttempts));
            IsPublisherDefined = true;
            return this;
        }
    }
}
