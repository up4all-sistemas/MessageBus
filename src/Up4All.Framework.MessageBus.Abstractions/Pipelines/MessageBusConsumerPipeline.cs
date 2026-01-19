using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Pipelines
{
    public abstract class MessageBusFlowPipeline<TPipeline, TOptions>(TPipeline mainPipeline)
    {
        public TPipeline MainPipeline { get; set; } = mainPipeline;

        public TPipeline Then()
        {
            return MainPipeline;
        }
    }

    public abstract class MessageBusPublisherPipeline<TPipeline, TOptions>(TPipeline mainPipeline)
        : MessageBusFlowPipeline<TPipeline, TOptions>(mainPipeline), IPublishPipelineBuilder
        where TOptions : MessageBusOptions
        where TPipeline : MessageBusPipeline<TOptions>
    {
        protected bool IsPublisherDefined = false;

        public void Validate()
        {
            if (!IsPublisherDefined)
                throw new ArgumentException("Não foi encontrada instância para a interface IMessageBusPublisherAsync");
        }
    }

    public abstract class MessageBusConsumerPipeline<TPipeline, TOptions>(TPipeline mainPipeline)
        : MessageBusFlowPipeline<TPipeline, TOptions>(mainPipeline), IConsumerPipelineBuilder
        where TOptions : MessageBusOptions
        where TPipeline : MessageBusPipeline<TOptions>
    {
        protected bool IsHandlerDefined = false;

        public IConsumerPipelineBuilder AddHandler<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            IsHandlerDefined = true;
            return this;
        }

        public IConsumerPipelineBuilder AddHandler<TMessageBusMessageHandler>(Func<IServiceProvider, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            MainPipeline.Services.AddTransient<IMessageBusMessageHandler>(builder);
            IsHandlerDefined = true;
            return this;
        }

        public IConsumerPipelineBuilder AddHostedService<THostedService>()
            where THostedService : class, IMessageDefaultConsumer
        {
            MainPipeline.Services.RemoveAll<IMessageDefaultConsumer>();
            MainPipeline.Services.AddHostedService<THostedService>();
            return this;
        }

        public abstract IConsumerPipelineBuilder AddDefaultHostedService();

        public void Validate()
        {
            if (!IsHandlerDefined)
                throw new ArgumentException("Não foi associada uma implementação para o handler IMessageBusMessageHandler");
        }

        
    }


}
