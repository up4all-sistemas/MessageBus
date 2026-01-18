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
        public static void AddMessageBusOptions(this IServiceCollection services, string configurationKey = "MessageBusOptions")
        {
            services.AddMessageBusOptions<MessageBusOptions>(configurationKey);
        }

        public static void AddMessageBusOptions<TOptions>(this IServiceCollection services, string configurationKey = "MessageBusOptions")
            where TOptions : MessageBusOptions, new()
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(configurationKey)
                .ValidateDataAnnotations();
        }

        private static void AddInstanceFactory<TMessageBusOptions>(this IServiceCollection services, string configurationBindkey = "MessageBusOptions")
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusOptions<TMessageBusOptions>(configurationBindkey);
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

        public static void AddMessageBusNamedQueueAsyncClient(this IServiceCollection services, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneQueueAsyncClient> createInstance, string configurationBindkey = "MessageBusOptions")
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedQueueAsyncClient<TClient, TMessageBusOptions>(this IServiceCollection services, string key
            , Func<IServiceProvider, TMessageBusOptions, TClient> createInstance, string configurationBindkey = "MessageBusOptions")
            where TClient : class, IMessageBusStandaloneQueueAsyncClient
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedStreamAsyncClient(this IServiceCollection services, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneStreamAsyncClient> createInstance, string configurationBindkey = "MessageBusOptions")
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedStreamAsyncClient<TClient,TMessageBusOptions>(this IServiceCollection services, string key
            , Func<IServiceProvider, TMessageBusOptions, TClient> createInstance, string configurationBindkey = "MessageBusOptions")
            where TClient : class, IMessageBusStandaloneStreamAsyncClient
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedTopicAsyncClient(this IServiceCollection services, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandalonePublisherAsync> createInstance, string configurationBindkey = "MessageBusOptions")
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedTopicAsyncClient<TClient,TMessageBusOptions>(this IServiceCollection services, string key, Func<IServiceProvider, TMessageBusOptions, TClient> createInstance, string configurationBindkey = "MessageBusOptions")
            where TClient : class, IMessageBusStandalonePublisherAsync
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedSubscriptionAsyncClient(this IServiceCollection services, string key, Func<IServiceProvider, MessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance, string configurationBindkey = "MessageBusOptions")
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        public static void AddMessageBusNamedSubscriptionAsyncClient<TMessageBusOptions>(this IServiceCollection services, string key, Func<IServiceProvider, TMessageBusOptions, IMessageBusStandaloneAsyncConsumer> createInstance, string configurationBindkey = "MessageBusOptions")
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddMessageBusNamedClient(key, createInstance, configurationBindkey);
        }

        private static void AddMessageBusNamedClient<TClient, TMessageBusOptions>(this IServiceCollection services, string key, Func<IServiceProvider, TMessageBusOptions, TClient> createInstance, string configurationBindkey = "MessageBusOptions")
            where TClient : class
            where TMessageBusOptions : MessageBusOptions, new()
        {
            services.AddInstanceFactory<TMessageBusOptions>(configurationBindkey);

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

        private static void AddMessageBusNamedClient<TClient>(this IServiceCollection services, string key, Func<IServiceProvider, MessageBusOptions, TClient> createInstance, string configurationBindkey = "MessageBusOptions")
            where TClient : class
        {
            services.AddMessageBusNamedClient<TClient, MessageBusOptions>(key, createInstance, configurationBindkey);
        }
    }
}
