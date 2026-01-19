using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.ServiceBus.Consumers;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusQueuePipeline(ServiceBusMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<ServiceBusMessageBusPipeline, MessageBusOptions>(pipeline)
    {
        public ServiceBusMessageBusQueuePipeline ListenQueue()
        {

            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusQueueAsyncClient>();

            return this;
        }

        public ServiceBusMessageBusQueuePipeline ListenQueue(string connectionString, string queueName
            , int connectionAttempts = 8)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneQueueAsyncClient>>();
                return new ServiceBusStandaloneQueueAsyncClient(logger, connectionString, queueName, connectionAttempts);
            });
            IsHandlerDefined = true;
            return this;
        }

        public override IConsumerPipelineBuilder AddDefaultHostedService()
        {
            AddHostedService<ServiceBusDefaultConsumer>();
            return this;
        }

    }
}
