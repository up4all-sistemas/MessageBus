using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        private const string _configurationBindKey = "MessageBusOptions";

        public static IServiceCollection AddServiceBusQueueAsyncClient(this IServiceCollection services, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusOptions(configurationBindKey);
            services.AddSingleton<IMessageBusQueueAsyncClient, ServiceBusQueueAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusTopicAsyncClient(this IServiceCollection services, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusOptions(configurationBindKey);
            services.AddSingleton<IMessageBusPublisherAsync, ServiceBusTopicAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusSubscriptionAsyncClient(this IServiceCollection services, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusOptions(configurationBindKey);
            services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusSubscribeAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusStandaloneQueueAsyncClient<TMessageBusStandaloneQueueAsyncClient>(this IServiceCollection services
            , Func<IServiceProvider, TMessageBusStandaloneQueueAsyncClient> builder)            
            where TMessageBusStandaloneQueueAsyncClient : IMessageBusStandaloneQueueAsyncClient, new()
        {
            return services.AddSingleton(builder);
        }

        public static IServiceCollection AddServiceBusStandaloneSubscriptionAsyncClient<TMessageBusStandaloneAsyncConsumer>(this IServiceCollection services
            , Func<IServiceProvider, TMessageBusStandaloneAsyncConsumer> builder)
            where TMessageBusStandaloneAsyncConsumer : IMessageBusStandaloneAsyncConsumer, new()
        {
            return services.AddSingleton(builder);
        }

        public static IServiceCollection AddServiceBusStandaloneTopicAsyncClient<TMessageBusStandalonePublisherAsync>(this IServiceCollection services
            , Func<IServiceProvider, TMessageBusStandalonePublisherAsync> builder)
            where TMessageBusStandalonePublisherAsync : IMessageBusStandalonePublisherAsync, new()
        {
            return services.AddSingleton(builder);
        }

        public static IServiceCollection AddServiceBusNamedQueueAsyncClient(this IServiceCollection services, string key, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusNamedQueueAsyncClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneQueueAsyncClient(provider.GetLogger<ServiceBusStandaloneQueueAsyncClient>(), opts.ConnectionString, key);
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddServiceBusNamedTopicAsyncClient(this IServiceCollection services, string key, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusNamedTopicAsyncClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneTopicAsyncClient(provider.GetLogger<ServiceBusStandaloneTopicAsyncClient>(), opts.ConnectionString, key, opts.ConnectionAttempts);
            }, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddServiceBusNamedSubscriptionAsyncClient(this IServiceCollection services, string key, string configurationBindKey = _configurationBindKey)
        {
            services.AddMessageBusNamedSubscriptionAsyncClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneSubscribeAsyncClient(provider.GetLogger<ServiceBusStandaloneSubscribeAsyncClient>(), opts.ConnectionString, opts.TopicName, key);
            }, configurationBindKey);
            return services;
        }

        private static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            return provider.GetRequiredService<ILogger<T>>();
        }
    }
}
