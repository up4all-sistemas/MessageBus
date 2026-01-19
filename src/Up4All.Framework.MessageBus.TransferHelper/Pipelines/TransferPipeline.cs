using Microsoft.Extensions.DependencyInjection;

using System;

using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.TransferHelper.Handlers;
using Up4All.Framework.MessageBus.TransferHelper.Options;
using Up4All.Framework.MessageBus.TransferHelper.Transformations;

namespace Up4All.Framework.MessageBus.TransferHelper.Pipelines
{
    public class TransferPipeline<TSourceOptions, TDestinationOptions>
        : MessageBusPipeline<TransferOptions<TSourceOptions, TDestinationOptions>>
        where TSourceOptions : MessageBusOptions
        where TDestinationOptions : MessageBusOptions, new()
    {
        private IPublishPipelineBuilder? _publisherPipeline = null;
        private IConsumerPipelineBuilder? _consumerPipelineBuilder = null;

        public TransferPipeline(IServiceCollection services, string configurationBindKey)
            : base(services, configurationBindKey)
        {
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddSource(
              Func<IConsumerPipelineBuilder> builderSource)
        {
            _consumerPipelineBuilder = builderSource();
            _consumerPipelineBuilder.AddHandler<ConsumerHandler<TSourceOptions, TDestinationOptions>>()
                          .AddDefaultHostedService();

            AddBeforeTransferHandler<DefaultBeforeTransferHandler>();
            AddTransferTransformationHandler<DefaultTransformationHandler>();

            return this;
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddDestination(Func<IPublishPipelineBuilder> builderDestination)
        {
            _publisherPipeline = builderDestination();
            return this;
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddTransferTransformationHandler<TBeforeTransferHandler>()
            where TBeforeTransferHandler : class, ITransformationHandler
        {
            Services.AddTransient<ITransformationHandler, TBeforeTransferHandler>();
            return this;
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddBeforeTransferHandler<TBeforeTransferHandler>()
            where TBeforeTransferHandler : class, IBeforeTransferHandler
        {
            Services.AddTransient<IBeforeTransferHandler, TBeforeTransferHandler>();
            return this;
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddHostedService()
        {
            _consumerPipelineBuilder!.AddDefaultHostedService();
            return this;
        }

        public TransferPipeline<TSourceOptions, TDestinationOptions> AddHostedService<THostedService>()
            where THostedService : class, IMessageDefaultConsumer
        {
            _consumerPipelineBuilder!.AddHostedService<THostedService>();
            return this;
        }

        public override void Validate()
        {
            _publisherPipeline?.Validate();
            _consumerPipelineBuilder?.Validate();
        }

    }
}
