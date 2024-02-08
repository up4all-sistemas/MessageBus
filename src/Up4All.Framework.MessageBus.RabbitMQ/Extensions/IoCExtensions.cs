using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddRabbitMQQueueClient(this IServiceCollection services, IConfiguration configuration
            , QueueDeclareOptions declareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusQueueClient, RabbitMQQueueClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQQueueClient>>();
                return new RabbitMQQueueClient(options, logger, null, declareOpts);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQStreamClient(this IServiceCollection services, IConfiguration configuration, object offset
            , StreamDeclareOptions declareOpts = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusStreamClient, RabbitMQStreamClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQStreamClient>>();
                return new RabbitMQStreamClient(options, logger, offset, declareOpts);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQTopicClient(this IServiceCollection services, IConfiguration configuration, string type = "classic"
            , ExchangeDeclareOptions declareOptions = null)
        {
            services.AddConfigurationBinder(configuration);

            services.AddSingleton<IMessageBusPublisher, RabbitMQTopicClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                var logger = provider.GetRequiredService<ILogger<RabbitMQTopicClient>>();
                return new RabbitMQTopicClient(logger, options, type, declareOptions);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMQNamedStreamClient(this IServiceCollection services, string key, IConfiguration configuration, object offset
            , StreamDeclareOptions declareOpts = null)
        {
            services.AddMessageBusNamedStreamClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, key, offset, declareOpts: declareOpts);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedQueueClient(this IServiceCollection services, string key, IConfiguration configuration
            , QueueDeclareOptions declareOptions = null)
        {
            services.AddMessageBusNamedQueueClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, key, declareOpts: declareOptions);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQNamedTopicClient(this IServiceCollection services, string key, IConfiguration configuration, string type = "classic"
            , ExchangeDeclareOptions declareOptions = null)
        {
            services.AddMessageBusNamedTopicClient(configuration, key, (provider, opts) =>
            {
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, key, 8, type, declareOptions);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneQueueClient(this IServiceCollection services, IConfiguration configuration
            , QueueDeclareOptions declareOpts = null)
        {
            services.AddStandaloneQueueClient((provider) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneQueueClient(opts.ConnectionString, opts.QueueName, declareOpts: declareOpts);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneTopicClient(this IServiceCollection services, IConfiguration configuration, string type
            , ExchangeDeclareOptions declareOptions = null)
        {
            services.AddStandaloneTopicClient((provider) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneTopicClient(opts.ConnectionString, opts.TopicName, type: type, declareOpts: declareOptions);
            });
            return services;
        }

        public static IServiceCollection AddRabbitMQStandaloneStreamClient(this IServiceCollection services, IConfiguration configuration, object offset
            , StreamDeclareOptions declareOpts = null)
        {
            services.AddStandaloneStreamClient(offset, (provider, x) =>
            {
                var opts = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                return new RabbitMQStandaloneStreamClient(opts.ConnectionString, opts.StreamName, x, declareOpts: declareOpts);
            });
            return services;
        }
    }

    public class RabbitMQConsts
    {
        public static QueueDeclareOptions DefaultQueueDeclareOptions = new QueueDeclareOptions();
        public static StreamDeclareOptions DefaultStreamDeclareOptions = new StreamDeclareOptions();
        public static ExchangeDeclareOptions DefaultExchangeDeclareOptions = new ExchangeDeclareOptions();
    }
}
