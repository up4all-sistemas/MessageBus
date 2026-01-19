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

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync, KafkaTopicAsyncClient>();

            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageKey, TMessageBusMessageHandler>()
            where TMessageKey : class
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync, KafkaGenericTopicAsyncClient<TMessageKey>>();

            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageBusMessageHandler>(string connectionString, string topicName
            , int connectionAttempts = 8)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp => new KafkaStandaloneTopicAsyncClient(connectionString, topicName, connectionAttempts));

            IsPublisherDefined = true;
            return this;
        }

        public KafkaMessageBusPublisherPipeline AddPublisher<TMessageKey, TMessageBusMessageHandler>(string connectionString, string topicName
            , int connectionAttempts = 8)
            where TMessageKey : class
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp => new KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(connectionString, topicName, connectionAttempts));

            IsPublisherDefined = true;
            return this;
        }
    }
}
