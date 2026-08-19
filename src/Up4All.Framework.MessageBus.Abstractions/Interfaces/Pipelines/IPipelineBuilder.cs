using System;
using Up4All.Framework.MessageBus.Abstractions.Consumers;
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

        IConsumerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object serviceKey)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IConsumerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object servicekey, Func<IServiceProvider, object?, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IConsumerPipelineBuilder AddDefaultHostedService();

        IConsumerPipelineBuilder AddHostedService<THostedService>()
            where THostedService : class, IMessageDefaultConsumer;

        public IConsumerPipelineBuilder AddKeyedHostedService<THostedService>(object serviceKey)
            where THostedService : DefaultKeyedConsumer;

        public IConsumerPipelineBuilder AddDefaultKeyedHostedService(object serviceKey);
    }
}
