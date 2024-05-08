using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Mocks.Extensions
{
    public static class MockExtensions
    {
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

        public static void AddStandaloneMessageBusQueueClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneQueueClientMock
        {
            services.AddSingleton<IMessageBusStandaloneQueueClient, T>();
        }

        public static void AddStandaloneMessageBusTopicClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneTopicClientMock
        {
            services.AddSingleton<IMessageBusStandalonePublisher, T>();
        }

        public static void AddStandaloneMessageBusSubscribeClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneSubscribeClientMock
        {
            services.AddSingleton<IMessageBusStandaloneConsumer, T>();
        }

        public static void AddStandaloneMessageBusStreamClientMocked<T>(this IServiceCollection services) where T : MessageBusStandaloneStreamClientMock
        {
            services.AddSingleton<IMessageBusStandaloneStreamClient, T>();
        }
    }
}
