using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections;
using System.Collections.Generic;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddRabbitMQQueueClient(this IServiceCollection services, IConfiguration configuration, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusQueueClient, RabbitMQQueueClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQQueueClient>>();
                return new RabbitMQQueueClient(options, logger, null, exclusive, durable, autoDelete, args);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusStreamClient, RabbitMQStreamClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQStreamClient>>();
                return new RabbitMQStreamClient(options, logger, offset, exclusive, durable, autoDelete, args);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicClient(this IServiceCollection services, IConfiguration configuration, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusPublisher, RabbitMQTopicClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQTopicClient>>();
                return new RabbitMQTopicClient(logger, options);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamClient(this IServiceCollection services, string key, IConfiguration configuration, object offset, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string,object> args = null)
        {
            services.AddMessageBusNamedStreamClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, key, offset, exclusive: exclusive, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddMessageBusNamedQueueClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, key, exclusive: exclusive, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration, string type, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, type: type, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddStandaloneQueueClient((provider) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, exclusive: exclusive, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddStandaloneTopicClient((provider) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset, bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null)
        {
            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, exclusive: exclusive, durable: durable, autoDelete: autoDelete, args: args);
            });
            return services;
        }
    }
}
