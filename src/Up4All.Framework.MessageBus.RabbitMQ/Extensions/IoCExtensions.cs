using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using System;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class IoCExtensions
    {
        #region Standard Clients

        public static IServiceCollection AddRabbitMQQueueAsyncClient(this IServiceCollection services
            , Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusQueueAsyncClient, RabbitMQQueueAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQQueueAsyncClient(provider.GetLogger<RabbitMQQueueAsyncClient>(), options, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamAsyncClient(this IServiceCollection services, object offset
            , Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusStreamAsyncClient, RabbitMQStreamAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQStreamAsyncClient(provider.GetLogger<RabbitMQStreamAsyncClient>(), options, offset, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicAsyncClient(this IServiceCollection services, string type = ExchangeType.Direct
            , Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusPublisherAsync, RabbitMQTopicAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQTopicAsyncClient(provider.GetLogger<RabbitMQTopicAsyncClient>(), options, type, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }


        #endregion

        #region Standalone Clients

        public static IServiceCollection AddRabbitMQStandaloneQueueAsyncClient(this IServiceCollection services
            , Func<IServiceProvider, RabbitMQStandaloneQueueAsyncClient> builder)
        {
            services.AddSingleton<IMessageBusStandaloneQueueAsyncClient>(builder);
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamAsyncClient(this IServiceCollection services
            , Func<IServiceProvider, RabbitMQStandaloneStreamAsyncClient> builder)
        {
            services.AddSingleton<IMessageBusStandaloneStreamAsyncClient>(builder);
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicAsyncClient(this IServiceCollection services
            , Func<IServiceProvider, RabbitMQStandaloneTopicAsyncClient> builder)
        {
            services.AddSingleton<IMessageBusStandalonePublisherAsync>(builder);
            return services;
        }

        #endregion

        #region Named Clients

        public static IServiceCollection AddRabbitMQNamedStreamAsyncClient(this IServiceCollection services, string key, object offset
            , Action<StreamDeclareOptions> configureDeclareOpts = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedStreamAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneStreamAsyncClient(provider.GetLogger<RabbitMQStandaloneStreamAsyncClient>(), opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamAsyncClient(this IServiceCollection services, string key, object offset
            , Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedStreamAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneStreamAsyncClient(provider.GetLogger<RabbitMQStandaloneStreamAsyncClient>(), opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueAsyncClient(this IServiceCollection services, string key
            , Action<QueueDeclareOptions> configureDeclareOpts = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedQueueAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueAsyncClient(provider.GetLogger<RabbitMQQueueAsyncClient>(), opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueAsyncClient(this IServiceCollection services, string key
            , Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedQueueAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueAsyncClient(provider.GetLogger<RabbitMQQueueAsyncClient>(), opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, string type
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedTopicAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicAsyncClient(provider.GetLogger<RabbitMQStandaloneTopicAsyncClient>(), opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, string type
            , Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts, string configurationBindKey = "MessageBusOptions")
        {
            services.AddMessageBusNamedTopicAsyncClient(key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicAsyncClient(provider.GetLogger<RabbitMQStandaloneTopicAsyncClient>(), opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(provider, configureDeclareOpts));
            }, configurationBindKey);
            return services;
        }

        #endregion

        #region Telemetry

        public static TracerProviderBuilder AddOpenTelemetryForMessageBus(this TracerProviderBuilder builder)
        {
            builder.AddSource(Consts.OpenTelemetrySourceName);
            return builder;
        }

        #endregion

        #region Private Methods

        private static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            return provider.GetRequiredService<ILogger<T>>();
        }

        private static ExchangeDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, RabbitMQMessageBusOptions options, Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToExchangeDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(provider, options, declareOpts ?? RabbitMQConsts.DefaultExchangeDeclareOptions);

            return declareOpts;
        }

        private static ExchangeDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static ExchangeDeclareOptions ConfigureDeclareOpts(RabbitMQMessageBusOptions options, Action<RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToExchangeDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(options, declareOpts ?? RabbitMQConsts.DefaultExchangeDeclareOptions);

            return declareOpts;
        }

        private static ExchangeDeclareOptions ConfigureDeclareOpts(Action<ExchangeDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, RabbitMQMessageBusOptions options, Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToStreamDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(provider, options, declareOpts ?? RabbitMQConsts.DefaultStreamDeclareOptions);

            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(RabbitMQMessageBusOptions options, Action<RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToStreamDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(options, declareOpts ?? RabbitMQConsts.DefaultStreamDeclareOptions);

            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(Action<StreamDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, RabbitMQMessageBusOptions options, Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToQueueDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(provider, options, declareOpts ?? RabbitMQConsts.DefaultQueueDeclareOptions);

            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(RabbitMQMessageBusOptions options, Action<RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts)
        {
            var declareOpts = RabbitMQConsts.ToQueueDeclare(options.ProvisioningOptions);

            if (configureDeclareOpts is not null)
                configureDeclareOpts(options, declareOpts ?? RabbitMQConsts.DefaultQueueDeclareOptions);

            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(Action<QueueDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        #endregion
    }

    public static class RabbitMQConsts
    {
        public static QueueDeclareOptions DefaultQueueDeclareOptions => new();

        public static StreamDeclareOptions DefaultStreamDeclareOptions => new();

        public static ExchangeDeclareOptions DefaultExchangeDeclareOptions => new();

        public static QueueDeclareOptions ToQueueDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

        public static StreamDeclareOptions ToStreamDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

        public static ExchangeDeclareOptions ToExchangeDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

    }
}
