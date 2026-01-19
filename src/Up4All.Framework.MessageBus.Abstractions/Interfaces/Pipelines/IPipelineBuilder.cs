using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines
{
    public interface IPipelineBuilder
    {
        void Validate();
    }

    public interface IPublishPipelineBuilder : IPipelineBuilder
    {
    }

    public interface IConsumerPipelineBuilder : IPipelineBuilder
    {
        IConsumerPipelineBuilder AddHandler<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IConsumerPipelineBuilder AddHandler<TMessageBusMessageHandler>(Func<IServiceProvider, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IConsumerPipelineBuilder AddDefaultHostedService();

        IConsumerPipelineBuilder AddHostedService<THostedService>()
            where THostedService : class, IMessageDefaultConsumer;
    }
}
