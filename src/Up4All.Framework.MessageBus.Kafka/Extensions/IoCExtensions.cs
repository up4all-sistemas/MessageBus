using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class IoCExtensions
    {
        public static IServiceCollection AddKafkaTopicAsyncClient(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaTopicAsyncClient<string>(configurationBindKey);
            return services;
        }

        public static IServiceCollection AddKafkaTopicAsyncClient<TMessageKey>(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusPublisherAsync, KafkaGenericTopicAsyncClient<TMessageKey>>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();
                return new KafkaGenericTopicAsyncClient<TMessageKey>(options);
            });

            return services;
        }

        public static IServiceCollection AddKafkaWithStructKeyTopicAsyncClient<TMessageKey>(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusPublisherAsync, KafkaWithStructKeyTopicAsyncClient<TMessageKey>>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();
                return new KafkaWithStructKeyTopicAsyncClient<TMessageKey>(options);
            });

            return services;
        }

        public static IServiceCollection AddKafkaSubscriptionAsyncClient(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaSubscriptionAsyncClient<string>(configurationBindKey);
            return services;
        }

        public static IServiceCollection AddKafkaSubscriptionAsyncClient<TMessageKey>(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusAsyncConsumer, KafkaGenericSubscriptionAsyncClient<TMessageKey>>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();
                return new KafkaGenericSubscriptionAsyncClient<TMessageKey>(options);
            });

            return services;
        }

        public static IServiceCollection AddKafkaWithStructKeySubscriptionAsyncClient<TMessageKey>(this IServiceCollection services, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddSingleton<IMessageBusAsyncConsumer, KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();
                return new KafkaWithStructKeySubscriptionAsyncClient<TMessageKey>(options);
            });

            return services;
        }

        public static IServiceCollection AddKafkaStandaloneTopicAsyncClient<TMessageKey>(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneGenericTopicAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddStandaloneTopicAsyncClient(provider =>
            {
                if (builder is not null)
                    return builder(provider);

                var opts = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();

                return new KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(opts.Value.ConnectionString, opts.Value.TopicName, opts.Value.ConnectionAttempts);
            });

            return services;
        }

        public static IServiceCollection AddKafkaStandaloneTopicAsyncClient(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneTopicAsyncClient>? builder = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaStandaloneTopicAsyncClient<string>(builder, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddKafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddStandaloneTopicAsyncClient(provider =>
            {
                if (builder is not null)
                    return builder(provider);

                var opts = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();

                return new KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>(opts.Value.ConnectionString, opts.Value.TopicName, opts.Value.ConnectionAttempts);
            });

            return services;
        }

        public static IServiceCollection AddKafkaStandaloneSubscriptionAsyncClient<TMessageKey>(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddStandaloneSubscriptionAsyncClient(provider =>
            {
                if (builder is not null)
                    return builder(provider);

                var opts = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();

                return new KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(opts.Value.ConnectionString, opts.Value.TopicName, opts.Value.SubscriptionName);
            });

            return services;
        }

        public static IServiceCollection AddKafkaStandaloneSubscriptionAsyncClient(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneSubscriptionAsyncClient>? builder = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaStandaloneSubscriptionAsyncClient<string>(builder, configurationBindKey);
            return services;
        }

        public static IServiceCollection AddKafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(this IServiceCollection services
            , Func<IServiceProvider, KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusOptions<KafkaMessageBusOptions>(configurationBindKey);

            services.AddStandaloneSubscriptionAsyncClient(provider =>
            {
                if (builder is not null)
                    return builder(provider);

                var opts = provider.GetRequiredService<IOptions<KafkaMessageBusOptions>>();

                return new KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(opts.Value.ConnectionString, opts.Value.TopicName, opts.Value.SubscriptionName);
            });

            return services;
        }

        public static IServiceCollection AddKafkaNamedTopicAsyncClient<TMessageKey>(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneGenericTopicAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusNamedTopicAsyncClient<KafkaStandaloneGenericTopicAsyncClient<TMessageKey>,KafkaMessageBusOptions>(name, (provider, opts) =>
            {
                if (builder is not null)
                    return builder(provider);

                return new KafkaStandaloneGenericTopicAsyncClient<TMessageKey>(opts.ConnectionString, opts.TopicName, opts.ConnectionAttempts);
            }, configurationBindKey);

            return services;
        }

        public static IServiceCollection AddKafkaNamedTopicAsyncClient(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneTopicAsyncClient>? builder = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaNamedTopicAsyncClient<string>(name, builder, configurationBindKey);

            return services;
        }

        public static IServiceCollection AddKafkaNamedWithStructKeyTopicAsyncClient<TMessageKey>(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusNamedTopicAsyncClient<KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>, KafkaMessageBusOptions>(name, (provider, opts) =>
            {
                if (builder is not null)
                    return builder(provider);

                return new KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey>(opts.ConnectionString, opts.TopicName, opts.ConnectionAttempts);
            }, configurationBindKey);

            return services;
        }

        public static IServiceCollection AddKafkaNamedWithStructSubscriptionAsyncClient<TMessageKey>(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : struct
        {
            services.AddMessageBusNamedSubscriptionAsyncClient<KafkaMessageBusOptions>(name, (provider, opts) =>
            {
                if (builder is not null)
                    return builder(provider);

                return new KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(opts.ConnectionString, opts.TopicName, opts.SubscriptionName);
            }, configurationBindKey);

            return services;
        }

        public static IServiceCollection AddKafkaNamedSubscriptionAsyncClient<TMessageKey>(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>? builder = null, string configurationBindKey = "MessageBusOptions")
            where TMessageKey : class
        {
            services.AddMessageBusNamedSubscriptionAsyncClient<KafkaMessageBusOptions>(name, (provider, opts) =>
            {
                if (builder is not null)
                    return builder(provider);

                return new KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(opts.ConnectionString, opts.TopicName, opts.SubscriptionName);
            }, configurationBindKey);

            return services;
        }


        public static IServiceCollection AddKafkaNamedSubscriptionAsyncClient(this IServiceCollection services, string name
            , Func<IServiceProvider, KafkaStandaloneSubscriptionAsyncClient>? builder = null, string configurationBindKey = "MessageBusOptions")
        {
            services.AddKafkaNamedSubscriptionAsyncClient<string>(name, builder, configurationBindKey);
            return services;
        }
    }
}
