using System;
using Up4All.Framework.MessageBus.Abstractions.Handlers;

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
    }

    public interface IHandlerPipelineBuilder : IPipelineBuilder
    {
        IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>(Func<IServiceProvider, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object serviceKey)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;

        IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object servicekey, Func<IServiceProvider, object?, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler;
    }

    
}
