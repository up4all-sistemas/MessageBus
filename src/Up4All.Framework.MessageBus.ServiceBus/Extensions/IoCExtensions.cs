using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {

        public static IServiceCollection AddServiceBusQueueAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusQueueAsyncClient, ServiceBusQueueAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusTopicAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusPublisherAsync, ServiceBusTopicAsyncClient>();
            return services;
        }

        public static IServiceCollection AddServiceBusSubscriptionAsyncClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusOptions(configuration);
            services.AddSingleton<IMessageBusAsyncConsumer, ServiceBusSubscribeAsyncClient>();
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

        public static IServiceCollection AddServiceBusNamedTopicAsyncClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedTopicAsyncClient(configuration, key, (provider, opts) =>
            {
                return new ServiceBusStandaloneTopicAsyncClient(opts.ConnectionString, key, opts.ConnectionAttempts);
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
