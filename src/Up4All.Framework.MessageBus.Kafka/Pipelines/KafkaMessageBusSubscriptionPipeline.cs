using Microsoft.Extensions.DependencyInjection;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Pipelines
{
    public class KafkaMessageBusHandlerPipelineBuilder(KafkaMessageBusPipeline pipeline)
        : MessageBusHandlerPipeline<KafkaMessageBusPipeline, KafkaMessageBusOptions>(pipeline)
    {
        public static IHandlerPipelineBuilder Create(KafkaMessageBusPipeline pipeline)
            => new KafkaMessageBusHandlerPipelineBuilder(pipeline);
    }

    public class KafkaMessageBusSubscriptionPipeline(KafkaMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<KafkaMessageBusPipeline>(pipeline)
    {
        public IHandlerPipelineBuilder ListenSubscription()
        {            
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaSubscriptionAsyncClient>();
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription<TMessageKey>()
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaGenericSubscriptionAsyncClient<TMessageKey>>();
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription(string connectionString, string topicName, string subscriptionName)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneSubscriptionAsyncClient(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription(object serviceKey, string connectionString, string topicName, string subscriptionName)
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp, key) => new KafkaStandaloneSubscriptionAsyncClient(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription<TMessageKey>(string connectionString, string topicName, string subscriptionName)
            where TMessageKey : class
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneWithGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription<TMessageKey>(object serviceKey, string connectionString, string topicName, string subscriptionName)
            where TMessageKey : class
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp,key) => new KafkaStandaloneWithGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscriptionWithStructKey<TMessageKey>()
            where TMessageKey : struct
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, KafkaWithStructKeySubscriptionAsyncClient<TMessageKey>>();
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscriptionWithStructKey<TMessageKey>(string connectionString, string topicName, string subscriptionName)
            where TMessageKey : struct
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp => new KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscriptionWithStructKey<TMessageKey>(object serviceKey, string connectionString, string topicName, string subscriptionName)
            where TMessageKey : struct
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp,key) => new KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName));
            return AddHandlerPipeline(KafkaMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }
    }
}
