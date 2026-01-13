using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using System;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class IoCExtensions
    {
        #region Standard Clients

        public static IServiceCollection AddRabbitMQQueueClient(this IServiceCollection services, IConfiguration configuration
            , Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusQueueClient, RabbitMQQueueClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQQueueClient(options, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQQueueAsyncClient(this IServiceCollection services, IConfiguration configuration
            , Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusQueueAsyncClient, RabbitMQQueueAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQQueueAsyncClient(options, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamClient(this IServiceCollection services, IConfiguration configuration, object offset
            , Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusStreamClient, RabbitMQStreamClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                //configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStreamClient(options, offset, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset
            , Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusStreamAsyncClient, RabbitMQStreamAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQStreamAsyncClient(options, offset, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicClient(this IServiceCollection services, IConfiguration configuration, string type = ExchangeType.Direct
            , Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusPublisher, RabbitMQTopicClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                //configureDeclareOpts?.Invoke(declareOpts);
                return new RabbitMQTopicClient(options, type, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type = ExchangeType.Direct
            , Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddSingleton<IMessageBusPublisherAsync, RabbitMQTopicAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                return new RabbitMQTopicAsyncClient(options, type, ConfigureDeclareOpts(provider, options.Value, configureDeclareOpts));
            });

            return services;
        }


        #endregion

        #region Standalone Clients

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration, Action<RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneQueueClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration, Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneQueueClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, Action<RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneQueueAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneQueueAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type, Action<RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneTopicClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type, Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneTopicClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type, Action<RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneTopicAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type, Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneTopicAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneStreamAsyncClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(opts, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusOptions<RabbitMQMessageBusOptions>(configuration);

            services.AddStandaloneStreamAsyncClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                var opts = provider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(provider, opts, configureDeclareOpts));
            });
            return services;
        }

        #endregion

        #region Named Clients

        public static IServiceCollection AddRabbitMQNamedStreamClient(this IServiceCollection services, string key, IConfiguration configuration, object offset
            , Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedStreamClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamClient(this IServiceCollection services, string key, IConfiguration configuration, object offset
            , Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedStreamClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, object offset
            , Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedStreamAsyncClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, object offset
            , Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedStreamAsyncClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, key, offset, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }



        public static IServiceCollection AddRabbitMQNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration
            , Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedQueueClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration
            , Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedQueueClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueAsyncClient(this IServiceCollection services, string key, IConfiguration configuration
            , Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedQueueAsyncClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueAsyncClient(this IServiceCollection services, string key, IConfiguration configuration
            , Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedQueueAsyncClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedTopicAsyncClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedTopicAsyncClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
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
