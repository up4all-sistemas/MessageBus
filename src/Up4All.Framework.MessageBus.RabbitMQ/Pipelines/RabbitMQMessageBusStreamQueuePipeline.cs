using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Pipelines
{
    public class RabbitMQMessageBusStreamQueuePipeline(RabbitMQMessageBusPipeline pipeline)
        : MessageBusConsumerPipeline<RabbitMQMessageBusPipeline>(pipeline)
    {
        public IHandlerPipelineBuilder ListenStreamQueue(Action<IServiceProvider, RabbitMQMessageBusOptions, StreamDeclareOptions>? queueDeclareBuilder = null
            , object? offset = null)
        {
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQStreamAsyncClient>>();

                var declareOpts = RabbitMQConsts.ToStreamDeclare(opts.Value.ProvisioningOptions);
                queueDeclareBuilder?.Invoke(sp, opts.Value, declareOpts);
                return new RabbitMQStreamAsyncClient(logger, opts, offset ?? OffsetType.Next, declareOpts);
            });
            return AddHandlerPipeline(RabbitMQMessageBusConsumerPipeline.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenStreamQueue(string connectionString, string queueName
            , object offset
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, StreamDeclareOptions>? queueDeclareBuilder = null)
        {            
            MainPipeline.Services.AddSingleton<IMessageBusAsyncConsumer>(sp
                => CreateClient(sp, connectionString, queueName, offset, persistent, connectionAttempts, queueDeclareBuilder));
            return AddHandlerPipeline(RabbitMQMessageBusConsumerPipeline.Create(MainPipeline));
        }

        public IHandlerPipelineBuilder ListenStreamQueue(object serviceKey, string connectionString, string queueName
            , object offset
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, StreamDeclareOptions>? queueDeclareBuilder = null)
        {
            MainPipeline.Services.AddKeyedSingleton<IMessageBusAsyncConsumer>(serviceKey, (sp,key)
                => CreateClient(sp, connectionString, queueName, offset, persistent, connectionAttempts, queueDeclareBuilder));
            return AddHandlerPipeline(RabbitMQMessageBusConsumerPipeline.Create(MainPipeline));
        }

        private static RabbitMQStandaloneStreamAsyncClient CreateClient(IServiceProvider sp, string connectionString, string queueName
            , object offset
            , bool persistent = true
            , int connectionAttempts = 8
            , Action<IServiceProvider, StreamDeclareOptions>? queueDeclareBuilder = null)
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQStandaloneStreamAsyncClient>>();

            var declareOpts = RabbitMQConsts.DefaultStreamDeclareOptions;
            queueDeclareBuilder?.Invoke(sp, declareOpts);
            return new RabbitMQStandaloneStreamAsyncClient(logger, connectionString, queueName, offset, persistent, connectionAttempts, declareOpts);
        }
    }
}
