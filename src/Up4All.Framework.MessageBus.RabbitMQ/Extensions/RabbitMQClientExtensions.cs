using Microsoft.Extensions.Logging;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class RabbitMQClientExtensions
    {
        public static ActivitySource ActivitySource => OpenTelemetryExtensions.CreateActivitySource<RabbitMQStandaloneQueueAsyncClient>();

        public static async Task ConfigureAsyncHandler(this IRabbitMQClient client, string queueName
            , AsyncQueueMessageReceiver receiver, bool autoComplete, CancellationToken cancellationToken
            , object offset = null)
        {
            await client.Channel.BasicQosAsync(0, 1, false, cancellationToken: cancellationToken);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            await client.Channel.BasicConsumeAsync(queueName, autoComplete, $"up4-{Environment.MachineName.ToLower()}", args, receiver, cancellationToken);
        }

        public static async Task ConfigureAsyncHandler<TModel>(this IRabbitMQClient client, string queueName, AsyncQueueMessageReceiverForModel<TModel> receiver
            , bool autoComplete, CancellationToken cancellationToken, object offset = null)
        {
            await client.Channel.BasicQosAsync(0, 1, false, cancellationToken: cancellationToken);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            await client.Channel.BasicConsumeAsync(queueName, autoComplete, $"up4-{Environment.MachineName.ToLower()}", args, receiver, cancellationToken);
        }

        public static async Task SendMessageAsync(this IChannel channel, ILogger logger, string topicName, string queueName, MessageBusMessage msg
            , bool persistent = true, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Sending message to {Target}", topicName ?? queueName);

            msg.AddTraceProperties("rabbitmq");

            var activityName = $"message-send {topicName} {queueName}";
            var basicProps = new BasicProperties();
            basicProps.PopulateHeaders(msg);

            if (msg.UserProperties.TryGetValue(Properties.IsPersistent, out var isPersistent))
                persistent = Convert.ToBoolean(isPersistent);

            basicProps.Persistent = persistent;
            basicProps.DeliveryMode = persistent ? DeliveryModes.Persistent : DeliveryModes.Transient;

            using var activity = ActivitySource.ProcessOpenTelemetryActivity(activityName, ActivityKind.Producer);
            var routingKey = queueName;

            if (msg.ContainsRoutingKey())
                routingKey = msg.GetRoutingKey();

            activity?.InjectPropagationContext(basicProps.Headers);
            activity?.AddTagsToActivity("rabbitmq", msg.Body, topicName, new Dictionary<string, object> {
                { "messaging.rabbitmq.routing_key", routingKey }
            });

            await channel.BasicPublishAsync(topicName, routingKey, false, basicProps, msg.Body, cancellationToken);
        }

        public static async Task<IConnection> GetConnectionAsync(this IRabbitMQClient client, string connectionString, int connectionAttempts, CancellationToken cancellationToken)
        {
            if (client.Connection != null) return client.Connection;

            IConnection conn = null;
            var result = await Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetryAsync(connectionAttempts, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAndCaptureAsync(async (c) =>
                {
                    if (conn != null && conn.IsOpen) return;
                    conn = await new ConnectionFactory() { Uri = new Uri(connectionString) }.CreateConnectionAsync(c);
                }, cancellationToken);

            if (result.Outcome != OutcomeType.Successful)
                throw result.FinalException;

            client.Connection = conn;
            return conn;
        }

        public static async Task<IChannel> CreateChannelAsync(this IRabbitMQClient client, CancellationToken cancellationToken)
        {
            if (client.Channel is not null) return client.Channel;
            return await client.Connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        public static async Task ConfigureQueueDeclareAsync(this IChannel channel, string queueName, QueueDeclareOptions declareOpts, CancellationToken cancellationToken)
        {
            if (declareOpts == null) return;

            if (!declareOpts.Args.ContainsKey("x-queue-type"))
                declareOpts.Args.Add("x-queue-type", declareOpts.Type);

            await channel.QueueDeclareAsync(queue: queueName, durable: declareOpts.Durable, exclusive: declareOpts.Exclusive, autoDelete: declareOpts.AutoDelete
                , arguments: declareOpts.Args, cancellationToken: cancellationToken);

            if (declareOpts.Bindings.Any())
                foreach (var bind in declareOpts.Bindings)
                    await channel.QueueBindAsync(queueName, bind.ExchangeName, bind.RoutingKey ?? ""
                        , bind.Args, cancellationToken: cancellationToken);

        }

        public static async Task ProcessMessageAsync(this IChannel channel, ulong deliveryTag, MessageReceivedStatus status, bool autoComplete, CancellationToken cancellationToken)
        {
            if (!autoComplete && status == MessageReceivedStatus.Deadletter)
            {
                await channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                return;
            }

            if (!autoComplete && status == MessageReceivedStatus.Abandoned)
            {
                await channel.BasicRejectAsync(deliveryTag, true, cancellationToken);
                return;
            }

            if (!autoComplete)
                await channel.BasicAckAsync(deliveryTag, false, cancellationToken);
        }

        public static async Task ProcessErrorMessageAsync(this IChannel _channel, ulong deliveryTag, bool autoComplete, CancellationToken cancellationToken)
        {
            if (!autoComplete)
                await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
        }

        #region Opentelemetry Extensions

        public static string CreateActivityName(this IAsyncBasicConsumer consumer, string activityName, string exchangeName, string routingKey)
        {
            return $"{activityName} {exchangeName} {routingKey} {consumer.Channel.CurrentQueue}";
        }

        public static string CreateMessageReceivedActivityName(this IAsyncBasicConsumer consumer, string exchangeName, string routingKey)
        {
            return consumer.CreateActivityName("message-received", exchangeName, routingKey);
        }

        public static Activity CreateMessageReceivedActivity(this IAsyncBasicConsumer consumer, IReadOnlyBasicProperties properties, string exchangeName, string routingKey)
        {
            var activityName = consumer.CreateMessageReceivedActivityName(exchangeName, routingKey);
            return ActivitySource.CreateActivity(properties.Headers, activityName, ActivityKind.Consumer);
        }


        #endregion
    }
}
