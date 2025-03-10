﻿using Microsoft.Extensions.Configuration;
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
            , Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusQueueClient, RabbitMQQueueClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQQueueClient(options, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQQueueAsyncClient(this IServiceCollection services, IConfiguration configuration
            , Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusQueueAsyncClient, RabbitMQQueueAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQQueueAsyncClient(options, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamClient(this IServiceCollection services, IConfiguration configuration, object offset
            , Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusStreamClient, RabbitMQStreamClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStreamClient(options, offset, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset
            , Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusStreamAsyncClient, RabbitMQStreamAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStreamAsyncClient(options, offset, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicClient(this IServiceCollection services, IConfiguration configuration, string type = ExchangeType.Direct
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusPublisher, RabbitMQTopicClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);
                return new RabbitMQTopicClient(options, type, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type = ExchangeType.Direct
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusPublisherAsync, RabbitMQTopicAsyncClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();

                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);
                return new RabbitMQTopicAsyncClient(options, type, ConfigureDeclareOpts(configureDeclareOpts));
            });

            return services;
        }


        #endregion

        #region Standalone Clients

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration, Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneQueueClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneQueueAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration, Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneQueueClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneQueueAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, opts.QueueName, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type, Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneTopicClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type, Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneTopicAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type, Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneTopicClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string type, Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneTopicAsyncClient((provider) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<StreamDeclareOptions> configureDeclareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneStreamAsyncClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, object offset, Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            services.AddConfigurationBinder(configuration);

            services.AddStandaloneStreamAsyncClient(offset, (provider, x) =>
            {
                var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamAsyncClient(opts.ConnectionString, opts.StreamName, x, declareOpts: ConfigureDeclareOpts(provider, configureDeclareOpts));
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

        public static IServiceCollection AddRabbitMQNamedQueueAsyncClient(this IServiceCollection services, string key, IConfiguration configuration
            , Action<QueueDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedQueueAsyncClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueAsyncClient(opts.ConnectionString, key, declareOpts: ConfigureDeclareOpts(configureDeclareOpts));
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
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<ExchangeDeclareOptions> configureDeclareOpts = null)
        {
            services.AddMessageBusNamedTopicAsyncClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts?.Invoke(declareOpts);

                return new RabbitMQStandaloneTopicAsyncClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, 8, type, ConfigureDeclareOpts(provider, configureDeclareOpts));
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicAsyncClient(this IServiceCollection services, string key, IConfiguration configuration, string type
            , Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            services.AddMessageBusNamedTopicAsyncClient(configuration, key, (provider, opts) =>
            {
                var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
                configureDeclareOpts.Invoke(provider, declareOpts);

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

        private static ExchangeDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, ExchangeDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new ExchangeDeclareOptions();
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static ExchangeDeclareOptions ConfigureDeclareOpts(Action<ExchangeDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new ExchangeDeclareOptions();
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, StreamDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new StreamDeclareOptions();
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static StreamDeclareOptions ConfigureDeclareOpts(Action<StreamDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new StreamDeclareOptions();
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(IServiceProvider provider, Action<IServiceProvider, QueueDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new QueueDeclareOptions();
            configureDeclareOpts(provider, declareOpts);
            return declareOpts;
        }

        private static QueueDeclareOptions ConfigureDeclareOpts(Action<QueueDeclareOptions> configureDeclareOpts)
        {
            if (configureDeclareOpts == null)
                return null;

            var declareOpts = new QueueDeclareOptions();
            configureDeclareOpts(declareOpts);
            return declareOpts;
        }

        #endregion
    }

    public static class RabbitMQConsts
    {
        public static QueueDeclareOptions DefaultQueueDeclareOptions => new ();
        public static StreamDeclareOptions DefaultStreamDeclareOptions => new ();
        public static ExchangeDeclareOptions DefaultExchangeDeclareOptions => new ();        
    }
}
