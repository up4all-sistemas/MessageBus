using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Configurations
{
    public static class RabbitMQConfiguration
    {
        public static IServiceCollection AddMessageBusQueueClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusQueueClient, RabbitMQQueueClient>();
            return services;
        }

        public static IServiceCollection AddMessageBusStreamClient(this IServiceCollection services, IConfiguration configuration, object offset)
        {
            services.AddMessageBusStreamClient<RabbitMQStreamClient>(configuration, offset, (logger, opts) => new RabbitMQStreamClient(opts, logger, offset));
            return services;
        }

        public static IServiceCollection AddMessageBusTopicClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusPublisher, RabbitMQTopicClient>();
            return services;
        }

        public static IServiceCollection AddMessageBusSubscriptionClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusConsumer, RabbitMQSubscriptionClient>();
            return services;
        }

        private static IServiceCollection AddConfigurationBinder(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessageBusOptions>(config => configuration.GetSection(nameof(MessageBusOptions)).Bind(config));
            return services;
        }
    }
}
