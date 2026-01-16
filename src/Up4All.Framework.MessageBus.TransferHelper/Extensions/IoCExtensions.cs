using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Handlers;
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

        public static TransferPipelineBuilder AddTransferClient<TSourceOptions, TDestinationOptions>(this IServiceCollection services
            , string configurationKey = "MessageBusTransferOptions")
            where TSourceOptions : MessageBusOptions, new()
            where TDestinationOptions : MessageBusOptions, new()
        {
            services.AddTransferTransformationHandler<DefaultTransformationHandler>();
            services.AddBeforeTransferHandler<DefaultBeforeTransferHandler>();
            services.AddTransferMessageBusOptions<TSourceOptions, TDestinationOptions>(configurationKey);
            services.AddSingleton<IMessageBusTransferClient, MessageBusTransferClient<TSourceOptions, TDestinationOptions>>();

            return new TransferPipelineBuilder(services);
        }

        public static TransferPipelineBuilder AddTransferClient<TSourceOptions,TDestinationOptions>(this IServiceCollection services
            , Func<IServiceProvider, TSourceOptions, IMessageBusAsyncConsumer> injectSource
            , Func<IServiceProvider, TDestinationOptions, IMessageBusPublisherAsync> injectDestination            
            , string configurationKey = "MessageBusTransferOptions")
            where TSourceOptions : MessageBusOptions, new()            
            where TDestinationOptions : MessageBusOptions, new()
        {
            services.AddTransferTransformationHandler<DefaultTransformationHandler>();
            services.AddBeforeTransferHandler<DefaultBeforeTransferHandler>();
            services.AddTransferMessageBusOptions<TSourceOptions, TDestinationOptions>(configurationKey);

            services.AddSingleton(sp => {
                var opts = sp.GetRequiredService<IOptions<TransferOptions<TSourceOptions,TDestinationOptions>>>();
                return injectSource(sp, opts.Value.Source);
            });

            services.AddSingleton(sp => {
                var opts = sp.GetRequiredService<IOptions<TransferOptions<TSourceOptions, TDestinationOptions>>>();
                return injectDestination(sp, opts.Value.Destination);
            });

            services.AddSingleton<IMessageBusTransferClient, MessageBusTransferClient<TSourceOptions,TDestinationOptions>>();

            return new TransferPipelineBuilder(services);
        }

        public static TransferPipelineBuilder AddTransferTransformationHandler<TBeforeTransferHandler>(this TransferPipelineBuilder pipeline)
            where TBeforeTransferHandler : class, ITransformationHandler
        {
            pipeline.Services.AddTransferTransformationHandler<TBeforeTransferHandler>();
            return pipeline;
        }

        public static TransferPipelineBuilder AddBeforeTransferHandler<TBeforeTransferHandler>(this TransferPipelineBuilder pipeline)
            where TBeforeTransferHandler : class, IBeforeTransferHandler
        {
            pipeline.Services.AddBeforeTransferHandler<TBeforeTransferHandler>();
            return pipeline;
        }

        public static TransferPipelineBuilder AddTransferHostedService(this TransferPipelineBuilder pipeline)
        {
            pipeline.Services.AddHostedService<MessageBusTransferHostedService>();
            return pipeline;
        }

        private static void AddTransferTransformationHandler<TTransferHandler>(this IServiceCollection services)
            where TTransferHandler : class, ITransformationHandler
        {
            services.AddTransient<ITransformationHandler, TTransferHandler>();
        }

        private static void AddBeforeTransferHandler<TBeforeTransferHandler>(this IServiceCollection services)
            where TBeforeTransferHandler : class, IBeforeTransferHandler
        {
            services.AddTransient<IBeforeTransferHandler, TBeforeTransferHandler>();
        }
    }

    public class TransferPipelineBuilder(IServiceCollection services)
    {
        public IServiceCollection Services { get; private set; } = services;
    }
}
