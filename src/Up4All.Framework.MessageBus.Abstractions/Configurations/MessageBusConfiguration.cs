using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Factories;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Configurations
{
    public static class MessageBusConfiguration
    {
        public static void AddConfigurationBinder(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessageBusOptions>(config => configuration.GetSection("MessageBusOptions").Bind(config));
        }

        private static void AddInstanceFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<MessageBusFactory>();
        }

        public static void AddMessageBusQueueClient<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IMessageBusQueueClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusQueueClient, T>();
        }

        public static void AddMessageBusStreamClient<T>(this IServiceCollection services, IConfiguration configuration, Func<ILogger<T>, IOptions<MessageBusOptions>, T> builder) where T : class, IMessageBusStreamClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusStreamClient, T>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<T>>();
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                return builder(logger, options);
            });
        }

        public static void AddMessageBusTopicClient<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IMessageBusPublisher
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusPublisher, T>();
        }

        public static void AddMessageBusSubscribeClient<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IMessageBusConsumer
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusConsumer, T>();
        }

        public static void AddStandaloneQueueClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneQueueClient> instance)
        {
            services.AddSingleton<IMessageBusStandaloneQueueClient>(instance);
        }

        public static void AddStandaloneQueueAsyncClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneQueueAsyncClient> instance)
        {
            services.AddSingleton(instance);
        }

        public static void AddStandaloneStreamClient(this IServiceCollection services, object offset, Func<IServiceProvider, object, IMessageBusStandaloneStreamClient> instance)
        {
            services.AddSingleton<IMessageBusStandaloneStreamClient>(provider => instance(provider, offset));
        }

        public static void AddStandaloneStreamAsyncClient(this IServiceCollection services, object offset, Func<IServiceProvider, object, IMessageBusStandaloneStreamAsyncClient> instance)
        {
            services.AddSingleton(provider => instance(provider, offset));
        }

        public static void AddStandaloneTopicClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandalonePublisher> instance)
        {
            services.AddSingleton<IMessageBusStandalonePublisher>(instance);
        }

        public static void AddStandaloneTopicAsyncClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandalonePublisherAsync> instance)
        {
            services.AddSingleton(instance);
        }

        public static void AddStandaloneTopicClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneConsumer> instance)
        {
            services.AddSingleton<IMessageBusStandaloneConsumer>(instance);
        }

        public static void AddMessageBusNamedQueueClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneQueueClient> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneQueueAsyncClient> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedStreamClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneStreamClient> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneStreamAsyncClient> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedTopicClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandalonePublisher> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandalonePublisherAsync> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedSubscriptionClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneConsumer> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedSubscriptionAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        private static void AddMessageBusNamedClient<TClient>(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, TClient> createInstance)
            where TClient : class
        {
            services.AddInstanceFactory(configuration);

            services.AddSingleton(p =>
            {
                var opts = p.GetRequiredService<IOptions<MessageBusOptions>>();

                var namedInstance = new NamedInstanceClient<TClient>
                {
                    Key = key,
                    Instance = createInstance(p, opts.Value)
                };
                return namedInstance;
            });
        }
    }
}
