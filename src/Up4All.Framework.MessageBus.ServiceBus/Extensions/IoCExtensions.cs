using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Configurations;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddMessageBusNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedQueueClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneQueueClient(opts.ConnectionString, key);
            });
            return services;
        }

        public static IServiceCollection AddMessageBusNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedTopicClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneTopicClient(opts.ConnectionString, key);
            });
            return services;
        }

        public static IServiceCollection AddMessageBusNamedSubscriptionClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedSubscriptionClient(key, (provider, opts) =>
            {
                return new ServiceBusStandaloneSubscribeClient(opts.ConnectionString, opts.TopicName, key);
            });
            return services;
        }
    }
}
