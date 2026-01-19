using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusPublisherPipeline(ServiceBusMessageBusPipeline pipeline)
        : MessageBusPublisherPipeline<ServiceBusMessageBusPipeline, MessageBusOptions>(pipeline)
    {
        public IPublishPipelineBuilder AddPublisher<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync, ServiceBusTopicAsyncClient>();
            IsPublisherDefined = true;
            return this;
        }

        public IPublishPipelineBuilder AddPublisher<TMessageBusMessageHandler>(string connectionString, string topicName
            , int connectionAttempts = 8)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusStandaloneTopicAsyncClient>>();
                return new ServiceBusStandaloneTopicAsyncClient(logger, connectionString, topicName, connectionAttempts);
            });

            IsPublisherDefined = true;
            return this;
        }
    }
}
