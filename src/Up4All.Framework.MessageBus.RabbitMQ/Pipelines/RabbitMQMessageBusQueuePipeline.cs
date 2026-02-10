using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Pipelines
{
    public class RabbitMQMessageBusQueuePipeline(RabbitMQMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<RabbitMQMessageBusPipeline, RabbitMQMessageBusOptions>(pipeline)
    {
        public RabbitMQMessageBusQueuePipeline ListenQueue(Action<IServiceProvider, RabbitMQMessageBusOptions, QueueDeclareOptions> queueDeclareBuilder = null)            
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQQueueAsyncClient>>();

                var declareOpts = RabbitMQConsts.ToQueueDeclare(opts.Value.ProvisioningOptions);
                queueDeclareBuilder?.Invoke(sp, opts.Value, declareOpts);
                return new RabbitMQQueueAsyncClient(logger, opts, declareOpts);
            });
            return this;
        }

        public RabbitMQMessageBusQueuePipeline ListenQueue(string connectionString, string queueName
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, QueueDeclareOptions> queueDeclareBuilder = null)            
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp
                => CreateClient(sp, connectionString, queueName, persistent, connectionAttempts, queueDeclareBuilder));            
            return this;
        }

        public override IConsumerPipelineBuilder AddDefaultHostedService()
        {
            AddHostedService<DefaultConsumer>();
            return this;
        }

        public IConsumerPipelineBuilder AddDefaultStreamHostedService()
        {
            AddHostedService<RabbitMQDefaultStreamConsumer>();
            return this;
        }

        private static RabbitMQStandaloneQueueAsyncClient CreateClient(IServiceProvider sp, string connectionString, string queueName
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, QueueDeclareOptions> queueDeclareBuilder = null)
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQStandaloneQueueAsyncClient>>();

            var declareOpts = RabbitMQConsts.DefaultQueueDeclareOptions;
            queueDeclareBuilder?.Invoke(sp, declareOpts);
            return new RabbitMQStandaloneQueueAsyncClient(logger, connectionString, queueName, persistent, connectionAttempts, declareOpts);
        }
    }
}
