using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Options;
using Up4All.Framework.MessageBus.TransferHelper.Transformations;

namespace Up4All.Framework.MessageBus.TransferHelper.Extensions
{
    public static class IoCExtensions
    {
        public static void AddTransferMessageBusOptions<TOptionsSource, TOptionsDestination>(this IServiceCollection services, string configurationKey = "MessageBusTransferOptions")
            where TOptionsSource : MessageBusOptions, new()
            where TOptionsDestination : MessageBusOptions, new()
        {
            services.AddOptions<TransferOptions<TOptionsSource, TOptionsDestination>>()
                .BindConfiguration(configurationKey)
                .ValidateDataAnnotations();
        }

        public static TransferPipelineBuilder AddTransferClient<TSourceConsumer,TSourceOptions,TDestinationPublisher,TDestinationOptions>(this IServiceCollection services
            , Func<IServiceProvider, TSourceOptions, TSourceConsumer> injectSource
            , Func<IServiceProvider, TDestinationOptions, TDestinationPublisher> injectDestination
            , Func<ReceivedMessage, CancellationToken, Task<MessageBusMessage>>? _funcTransform = null
            , string configurationKey = "MessageBusTransferOptions")
            where TSourceConsumer : class, IMessageBusAsyncConsumer
            where TSourceOptions : MessageBusOptions, new()
            where TDestinationPublisher : class, IMessageBusPublisherAsync
            where TDestinationOptions : MessageBusOptions, new()
        {
            services.AddTransferMessageBusOptions<TSourceOptions, TDestinationOptions>(configurationKey);

            services.AddSingleton<IMessageBusAsyncConsumer>(sp => {
                var opts = sp.GetRequiredService<IOptions<TransferOptions<TSourceOptions,TDestinationOptions>>>();
                return injectSource(sp, opts.Value.Source);
            });

            services.AddSingleton<IMessageBusPublisherAsync>(sp => {
                var opts = sp.GetRequiredService<IOptions<TransferOptions<TSourceOptions, TDestinationOptions>>>();
                return injectDestination(sp, opts.Value.Destination);
            });

            services.AddSingleton<IMessageBusTransferClient, MessageBusTransferClient>();

            return new TransferPipelineBuilder(services);
        }

        public static TransferPipelineBuilder AddTransferTransformationHandler<TTransferHandler>(this TransferPipelineBuilder pipeline)
            where TTransferHandler : class, ITransformationHandler
        {
            pipeline.Services.AddTransient<ITransformationHandler, TTransferHandler>();
            return pipeline;
        }

        public static TransferPipelineBuilder AddTransferHostedService(this TransferPipelineBuilder pipeline)
        {
            pipeline.Services.AddHostedService<MessageBusTransferHostedService>();
            return pipeline;
        }
    }

    public class TransferPipelineBuilder(IServiceCollection services)
    {
        public IServiceCollection Services { get; private set; } = services;
    }
}
