using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Configurations;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddMessageBusNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedQueueClient(key, (provider) =>
            {
                return new ServiceBusStandaloneQueueClient(configuration.GetValue<string>("MessageBusOptions:ConnectionString"), key);
            });
            return services;
        }

        public static IServiceCollection AddMessageBusNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedTopicClient(key, (provider) =>
            {
                return new ServiceBusStandaloneTopicClient(configuration.GetValue<string>("MessageBusOptions:ConnectionString"), key);
            });
            return services;
        }

        public static IServiceCollection AddMessageBusNamedSubscriptionClient(this IServiceCollection services, string key, IConfiguration configuration)
        {
            services.AddMessageBusNamedSubscriptionClient(key, (provider) =>
            {
                return new ServiceBusStandaloneSubscribeClient(configuration.GetValue<string>("MessageBusOptions:ConnectionString"), configuration.GetValue<string>("MessageBusOptions:TopicName"), key);
            });
            return services;
        }
    }
}
