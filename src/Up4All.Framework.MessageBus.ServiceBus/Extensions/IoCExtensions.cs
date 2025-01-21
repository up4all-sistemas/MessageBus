using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Configurations;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
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
