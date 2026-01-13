using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddServiceBusQueueClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusQueueClient, ServiceBusQueueClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusQueueAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusQueueAsyncClient, ServiceBusQueueAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusTopicClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusPublisher, ServiceBusTopicClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusTopicAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusPublisherAsync, ServiceBusTopicAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusSubscriptionClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusConsumer, ServiceBusSubscribeClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusSubscriptionAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusSubscribeAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedQueueClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneQueueClient(opts.ConnectionString, key);
            });
            return services;
        }

        public static IServiceCollection AddServiceBusNamedQueueAsyncClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedQueueAsyncClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneQueueAsyncClient(opts.ConnectionString, key);
            });
            return services;
        }

        public static IServiceCollection AddServiceBusNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneTopicClient(opts.ConnectionString, key);
            });
            return services;
        }

        public static IServiceCollection AddServiceBusNamedSubscriptionClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedSubscriptionClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneSubscribeClient(opts.ConnectionString, opts.TopicName, key);
            });
            return services;
        }

        public static IServiceCollection AddServiceBusNamedSubscriptionAsyncClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedSubscriptionAsyncClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneSubscribeAsyncClient(opts.ConnectionString, opts.TopicName, key); 
            });
            return services;
        }
    }
}
