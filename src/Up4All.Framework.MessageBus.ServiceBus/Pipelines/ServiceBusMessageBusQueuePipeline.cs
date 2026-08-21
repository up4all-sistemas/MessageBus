using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusHandlerPipelineBuilder(ServiceBusMessageBusPipeline pipeline)
        : MessageBusHandlerPipeline<ServiceBusMessageBusPipeline, MessageBusOptions>(pipeline)        
    {
        public static IHandlerPipelineBuilder Create(ServiceBusMessageBusPipeline pipeline)
            => new ServiceBusMessageBusHandlerPipelineBuilder(pipeline);
    }

    public class ServiceBusMessageBusQueuePipeline(ServiceBusMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<ServiceBusMessageBusPipeline>(pipeline)
    {
        public IHandlerPipelineBuilder ListenQueue()
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusQueueAsyncClient>();
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenQueue(string connectionString, string queueName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneQueueAsyncClient>>();
                return new ServiceBusStandaloneQueueAsyncClient(logger, connectionString, queueName, connectionAttempts);
            });
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenQueue(object serviceKey, string connectionString, string queueName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp,key) =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneQueueAsyncClient>>();
                return new ServiceBusStandaloneQueueAsyncClient(logger, connectionString, queueName, connectionAttempts);
            });
            return AddHandlerPipeline(ServiceBusMessageBusHandlerPipelineBuilder.Create(MainPipeline));
        }


    }
}
