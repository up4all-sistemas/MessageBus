using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using System;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.ServiceBus.Consumers;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusSubscriptionPipeline(ServiceBusMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<ServiceBusMessageBusPipeline, MessageBusOptions>(pipeline)
    {

        public IConsumerPipelineBuilder ListenSubscription()            
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusSubscriptionAsyncClient>();
            IsHandlerDefined = true;
            return this;
        }

        public IConsumerPipelineBuilder ListenSubscription(string connectionString, string topicName, string subscriptionName
            , int connectionAttempts = 8)            
        {            
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneSubscriptionAsyncClient>>();
                return new ServiceBusStandaloneSubscriptionAsyncClient(logger, connectionString, topicName, subscriptionName, connectionAttempts);
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
