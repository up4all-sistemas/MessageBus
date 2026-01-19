using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Pipelines
{
    public class RabbitMQMessageBusPublisherPipeline(RabbitMQMessageBusPipeline pipeline)
        : MessageBusPublisherPipeline<RabbitMQMessageBusPipeline, RabbitMQMessageBusOptions>(pipeline)
    {
        public RabbitMQMessageBusPublisherPipeline AddPublisher(string type = ExchangeType.Direct, Action<IServiceProvider, RabbitMQMessageBusOptions, ExchangeDeclareOptions> exchangeDeclareBuilder = null)

        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQTopicAsyncClient>>();

                var declareOpts = RabbitMQConsts.ToExchangeDeclare(opts.Value.ProvisioningOptions);
                exchangeDeclareBuilder?.Invoke(sp, opts.Value, declareOpts);
                return new RabbitMQTopicAsyncClient(logger, opts, type, declareOpts);
            });
            IsPublisherDefined = true;
            return this;
        }

        public RabbitMQMessageBusPublisherPipeline AddPublisher(string connectionString, string queueName
            , string type = ExchangeType.Direct
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, ExchangeDeclareOptions> exchangeDeclareBuilder = null)
        {
            MainPipeline.Services.AddSingleton<IMessageBusPublisherAsync>(sp => CreateInstance(sp, connectionString, queueName, type, persistent
                , connectionAttempts, exchangeDeclareBuilder));

            IsPublisherDefined = true;
            return this;
        }

        private static RabbitMQStandaloneTopicAsyncClient CreateInstance(IServiceProvider sp, string connectionString, string queueName
            , string type = ExchangeType.Direct
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, ExchangeDeclareOptions> exchangeDeclareBuilder = null)
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQStandaloneTopicAsyncClient>>();

            var declareOpts = RabbitMQConsts.DefaultExchangeDeclareOptions;
            exchangeDeclareBuilder?.Invoke(sp, declareOpts);
            return new RabbitMQStandaloneTopicAsyncClient(logger, connectionString, queueName, persistent, connectionAttempts, type, declareOpts);
        }
    }
}
