using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusSubscriptionPipeline(ServiceBusMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<ServiceBusMessageBusPipeline>(pipeline)
    {

        public IHandlerPipelineBuilder ListenSubscription()
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusSubscriptionAsyncClient>();
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription(string connectionString, string topicName, string subscriptionName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneSubscriptionAsyncClient>>();
                return new ServiceBusStandaloneSubscriptionAsyncClient(logger, connectionString, topicName, subscriptionName, connectionAttempts);
            });
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenSubscription(object serviceKey, string connectionString, string topicName, string subscriptionName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp,key) =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneSubscriptionAsyncClient>>();
                return new ServiceBusStandaloneSubscriptionAsyncClient(logger, connectionString, topicName, subscriptionName, connectionAttempts);
            });
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }
    }
}
