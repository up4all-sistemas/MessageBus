using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Factories;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Mocks;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Configurations
{
    public static class MessageBusConfiguration
    {
        private static void AddConfigurationBinder(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessageBusOptions>(config => configuration.GetSection("MessageBusOptions").Bind(config));
        }

        private static void AddInstanceFactory(this IServiceCollection services)
        {
            services.AddSingleton<MessageBusFactory>();
        }

        public static void AddMessageBusQueueClient<T>(this IServiceCollection services, IConfiguration configuration) where T : MessageBusQueueClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusQueueClient, T>();
        }

        public static void AddMessageBusStreamClient<T>(this IServiceCollection services, IConfiguration configuration, object offset, Func<ILogger<T>, IOptions<MessageBusOptions>, T> builder) where T : MessageBusStreamClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusStreamClient, T>(provider => 
            {
                var logger = provider.GetRequiredService<ILogger<T>>();
                var options = provider.GetRequiredService<IOptions<MessageBusOptions>>();
                return builder(logger, options);
            });
        }

        public static void AddMessageBusTopicClient<T>(this IServiceCollection services, IConfiguration configuration) where T : MessageBusTopicClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusPublisher, T>();
        }

        public static void AddMessageBusSubscribeClient<T>(this IServiceCollection services, IConfiguration configuration) where T : MessageBusSubscribeClient
        {
            services.AddConfigurationBinder(configuration);
            services.AddSingleton<IMessageBusConsumer, T>();
        }

        public static void AddMessageBusQueueClientMocked<T>(this IServiceCollection services) where T : MessageBusQueueClientMock
        {
            services.AddSingleton<IMessageBusQueueClient, T>();
        }

        public static void AddMessageBusStreamClientMocked<T>(this IServiceCollection services) where T : MessageBusStreamClientMock    
        {
            services.AddSingleton<IMessageBusStreamClient, T>();
        }

        public static void AddMessageBusTopicClientMocked<T>(this IServiceCollection services) where T : MessageBusTopicClientMock
        {
            services.AddSingleton<IMessageBusPublisher, T>();
        }

        public static void AddMessageBusSubscribeClientMocked<T>(this IServiceCollection services) where T : MessageBusSubscribeClientMock
        {
            services.AddSingleton<IMessageBusConsumer, T>();
        }

        public static void AddStandaloneQueueClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneQueueClient> instance)
        {
            services.AddSingleton<IMessageBusStandaloneQueueClient>(instance);
        }

        public static void AddStandaloneStreamClient(this IServiceCollection services, object offset, Func<IServiceProvider, object, MessageBusStandaloneStreamClient> instance)
        {
            services.AddSingleton<IMessageBusStandaloneStreamClient>(provider => instance(provider,offset));
        }

        public static void AddStandaloneTopicClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandalonePublisher> instance)
        {
            services.AddSingleton<IMessageBusStandalonePublisher>(instance);
        }

        public static void AddStandaloneTopicClient(this IServiceCollection services, Func<IServiceProvider, IMessageBusStandaloneConsumer> instance)
        {
            services.AddSingleton<IMessageBusStandaloneConsumer>(instance);
        }

        public static void AddStandaloneQueueClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneQueueClientMock
        {
            services.AddSingleton<IMessageBusStandaloneQueueClient, T>();
        }

        public static void AddStandaloneStreamClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneStreamClientMock
        {
            services.AddSingleton<IMessageBusStandaloneStreamClient, T>();
        }

        public static void AddStandaloneTopicClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneTopicClientMock
        {
            services.AddSingleton<IMessageBusStandalonePublisher, T>();
        }

        public static void AddStandaloneMessageBusSubscribeClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneSubscribeClientMock
        {
            services.AddSingleton<IMessageBusStandaloneConsumer, T>();
        }

        public static void AddMessageBusNamedQueueClient(this IServiceCollection services, string key, Func<IServiceProvider, IMessageBusStandaloneQueueClient> instance)
        {
            services.AddInstanceFactory();

            var namedInstance = new NamedInstanceClient<IMessageBusStandaloneQueueClient>();
            namedInstance.Key = key;
            namedInstance.Instance = instance;
            services.AddSingleton(namedInstance);
        }

        public static void AddMessageBusNamedTopicClient(this IServiceCollection services, string key, Func<IServiceProvider, IMessageBusStandalonePublisher> instance)
        {
            services.AddInstanceFactory();

            var namedInstance = new NamedInstanceClient<IMessageBusStandalonePublisher>();
            namedInstance.Key = key;
            namedInstance.Instance = instance;
            services.AddSingleton(namedInstance);
        }

        public static void AddMessageBusNamedSubscriptionClient(this IServiceCollection services, string key, Func<IServiceProvider, IMessageBusStandaloneConsumer> instance)
        {
            services.AddInstanceFactory();

            var namedInstance = new NamedInstanceClient<IMessageBusStandaloneConsumer>();
            namedInstance.Key = key;
            namedInstance.Instance = instance;
            services.AddSingleton(namedInstance);
        }
    }
}
