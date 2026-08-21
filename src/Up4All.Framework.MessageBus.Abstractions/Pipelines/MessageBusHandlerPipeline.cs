using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Pipelines
{
    public abstract class MessageBusFlowPipeline<TPipeline>(TPipeline mainPipeline)
        : IConsumerPipelineBuilder
    {
        public TPipeline MainPipeline { get; set; } = mainPipeline;

        public TPipeline Then()
        {
            return MainPipeline;
        }

        public abstract void Validate();        
    }

    public abstract class MessageBusPublisherPipeline<TPipeline, TOptions>(TPipeline mainPipeline)
        : MessageBusFlowPipeline<TPipeline>(mainPipeline), IPublishPipelineBuilder
        where TOptions : MessageBusOptions
        where TPipeline : MessageBusPipeline<TOptions>
    {
        protected bool IsPublisherDefined = false;

        public override void Validate()
        {
            if (!IsPublisherDefined)
                throw new ArgumentException("Não foi encontrada instância para a interface IMessageBusPublisherAsync");
        }
    }

    public abstract class MessageBusConsumerPipeline<TPipeline>(TPipeline mainPipeline)
        : MessageBusFlowPipeline<TPipeline>(mainPipeline), IPipelineBuilder
    {
        protected readonly List<IHandlerPipelineBuilder> Handlers = [];

        protected IHandlerPipelineBuilder AddHandlerPipeline(IHandlerPipelineBuilder handlerPipeline)
        {
            Handlers.Add(handlerPipeline);
            return handlerPipeline;
        }

        public override void Validate()
            => Handlers.ForEach(x => x.Validate());            
    }

    public abstract class MessageBusHandlerPipeline<TPipeline, TOptions>(TPipeline mainPipeline)
        : IHandlerPipelineBuilder
        where TOptions : MessageBusOptions
        where TPipeline : MessageBusPipeline<TOptions>
    {
        private readonly MessageBusPipeline<TOptions> _mainPipeline = mainPipeline;

        protected bool IsHandlerDefined = false;

        public IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            _mainPipeline.Services.AddTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>();
            _mainPipeline.Services.AddHostedService<DefaultConsumer<TMessageBusMessageHandler>>();
            IsHandlerDefined = true;
            return this;
        }

        public IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>(Func<IServiceProvider, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            _mainPipeline.Services.AddTransient<IMessageBusMessageHandler>(builder);
            _mainPipeline.Services.AddHostedService<DefaultConsumer<TMessageBusMessageHandler>>();
            IsHandlerDefined = true;
            return this;
        }

        public IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object serviceKey)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            _mainPipeline.Services.AddKeyedTransient<IMessageBusMessageHandler, TMessageBusMessageHandler>(serviceKey);
            AddKeyedHostedService<TMessageBusMessageHandler>(serviceKey);
            IsHandlerDefined = true;
            return this;
        }

        public IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object serviceKey, Func<IServiceProvider, object?, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
        {
            _mainPipeline.Services.AddKeyedTransient<IMessageBusMessageHandler>(serviceKey, builder);
            AddKeyedHostedService<TMessageBusMessageHandler>(serviceKey);
            IsHandlerDefined = true;
            return this;
        }

        protected IHandlerPipelineBuilder AddKeyedHostedService<THandler>(object serviceKey)
            where THandler : notnull, IMessageBusMessageHandler
        {
            _mainPipeline.Services.AddHostedService((sp) => new DefaultKeyedConsumer<THandler>(serviceKey, sp));
            return this;
        }

        protected IHandlerPipelineBuilder AddKeyedHostedService<THandler, TMessage>(object serviceKey)
            where THandler : notnull, IMessageBusMessageHandler<TMessage>
            where TMessage : class
        {
            _mainPipeline.Services.AddHostedService((sp) => new DefaultKeyedConsumer<THandler>(serviceKey, sp));
            return this;
        }

        public void Validate()
        {
            if (!IsHandlerDefined)
                throw new ArgumentException("Não foi associada uma implementação para o handler IMessageBusMessageHandler");
        }
    }


}
