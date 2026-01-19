using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.Kafka.Consumers;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Pipelines
{
    public class KafkaMessageBusSubscriptionPipeline(KafkaMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<KafkaMessageBusPipeline, KafkaMessageBusOptions>(pipeline)
    {
        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaSubscriptionAsyncClient>();
            IsHandlerDefined = true;
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageKey, TMessageBusMessageHandler>()
            where TMessageKey : class
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaGenericSubscriptionAsyncClient<TMessageKey>>();
            IsHandlerDefined = true;
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageBusMessageHandler>(string connectionString, string topicName, string subscriptionName)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneSubscriptionAsyncClient(connectionString, topicName, subscriptionName));
            IsHandlerDefined = true;
            return this;
        }

        public KafkaMessageBusSubscriptionPipeline ListenSubscription<TMessageKey, TMessageBusMessageHandler>(string connectionString, string topicName, string subscriptionName)
            where TMessageKey : class
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            IsHandlerDefined = true;
            return this;
        }

        public override IConsumerPipelineBuilder AddDefaultHostedService()
        {
            AddHostedService<KafkaMQDefaultConsumer>();
            return this;
        }
    }
}
