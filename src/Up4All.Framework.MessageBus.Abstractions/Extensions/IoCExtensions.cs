using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Factories;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    public static class IoCExtensions
    {
        public static void AddMessageBusOptions(this IServiceCollection services, IConfiguration configuration, string configurationKey = "MessageBusOptions")
        {
            services.AddMessageBusOptions<MessageBusOptions>(configuration, configurationKey);
        }

        public static void AddMessageBusOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string configurationKey = "MessageBusOptions")
            where TOptions : MessageBusOptions, new()
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(configurationKey)
                .ValidateDataAnnotations();
        }

        private static void AddInstanceFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<MessageBusFactory>();
        }

        public static void AddStandaloneQueueAsyncClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneQueueAsyncClient> instance)
        {
            services.AddSingleton(instance);
        }

        public static void AddStandaloneSubscriptionAsyncClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneAsyncConsumer> instance)
        {
            services.AddSingleton(instance);
        }

        public static void AddStandaloneStreamAsyncClient(this IServiceCollection services, object offset, Func<IServiceProvider, object, IMessageBusStandaloneAsyncConsumer> instance)
        {
            services.AddSingleton(provider => instance(provider, offset));
        }

        public static void AddStandaloneTopicAsyncClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandalonePublisherAsync> instance)
        {
            services.AddSingleton(instance);
        }

        public static void AddMessageBusNamedQueueAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneQueueAsyncClient> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedStreamAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedTopicAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandalonePublisherAsync> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedTopicAsyncClient<TMessageBusOptions>(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, TMessageBusOptions, IMessageBusStandalonePublisherAsync> createInstance)
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedSubscriptionAsyncClient(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance)
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        public static void AddMessageBusNamedSubscriptionAsyncClient<TMessageBusOptions>(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, TMessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance)
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(configuration, key, createInstance);
        }

        private static void AddMessageBusNamedClient<TClient, TMessageBusOptions>(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, TMessageBusOptions, TClient> createInstance)
            where TClient : class
            where TMessageBusOptions: MessageBusOptions, new()
        {
            services.AddInstanceFactory(configuration);
            services.AddMessageBusOptions<TMessageBusOptions>(configuration);

            services.AddSingleton(p =>
            {
                var opts = p.GetRequiredService<IOptions<TMessageBusOptions>>();

                var namedInstance = new NamedInstanceClient<TClient>
                {
                    Key = key,
                    Instance = createInstance(p, opts.Value)
                };
                return namedInstance;
            });
        }

        private static void AddMessageBusNamedClient<TClient>(this IServiceCollection services, IConfiguration configuration, string key, Func<IServiceProvider, MessageBusOptions, TClient> createInstance)
            where TClient : class
        {
            services.AddMessageBusNamedClient<TClient, MessageBusOptions>(configuration, key, createInstance);
        }
    }
}
