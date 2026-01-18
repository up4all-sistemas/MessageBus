using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class RabbitMQClientExtensions
    {
        private static readonly ActivitySource activitySource = new(Consts.OpenTelemetrySourceName);
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

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

        public static async Task SendMessageAsync(this IChannel channel, ILogger logger, string topicName, string queueName, MessageBusMessage msg, bool mandatory = false, bool persistent = true, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Sending message to {Target}", topicName ?? queueName);

            msg.AddUserProperty(Properties.Timestamp, DateTime.UtcNow.ToString("o"));
            msg.AddUserProperty(Properties.Provider, "rabbitmq");
            msg.AddUserProperty(Properties.MessageId, Guid.NewGuid().ToString());

            var activityName = $"message-send {topicName} {queueName}";
            var basicProps = new BasicProperties();
            basicProps.PopulateHeaders(msg);

            if (msg.UserProperties.TryGetValue(Properties.IsPersistent, out var isPersistent))
                persistent = Convert.ToBoolean(isPersistent);

            basicProps.Persistent = persistent;
            basicProps.DeliveryMode = persistent ? DeliveryModes.Persistent : DeliveryModes.Transient;

            using var activity = ProcessOpenTelemetryActivity(activityName, ActivityKind.Producer);
            var routingKey = queueName;

            if (msg.ContainsRoutingKey())
                routingKey = msg.GetRoutingKey();

            InjectPropagationContext(activity, basicProps);
            AddTagsToActivity(activity, topicName, routingKey, msg.Body);

            await channel.BasicPublishAsync(topicName, routingKey, mandatory, basicProps, msg.Body, cancellationToken);
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

        public static Activity ProcessOpenTelemetryActivity(string activityName, ActivityKind kind, ActivityContext parent = default)
        {
            var activity = activitySource.StartActivity(activityName, kind, parent);
            return activity;
        }

        public static Activity CreateActivity(this IReadOnlyBasicProperties properties, string activityName, ActivityKind kind)
        {
            var parentContext = GetParentPropagationContext(properties);
            var activity = ProcessOpenTelemetryActivity(activityName, kind, parentContext.ActivityContext);
            return activity;
        }

        public static Activity CreateMessageReceivedActivity(this IAsyncBasicConsumer consumer, IReadOnlyBasicProperties properties, string exchangeName, string routingKey)
        {
            var activityName = consumer.CreateMessageReceivedActivityName(exchangeName, routingKey);
            return properties.CreateActivity(activityName, ActivityKind.Consumer);
        }

        public static void InjectPropagationContext(Activity activity, IBasicProperties props)
        {
            if (activity == null) return;

            Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), props, (x, key, value) =>
            {
                props.Headers ??= new Dictionary<string, object>();
                props.Headers[key] = value;
            });
        }

        public static PropagationContext GetParentPropagationContext(IReadOnlyBasicProperties props)
        {
            var parent = Propagator.Extract(default, props, (x, key) =>
            {
                if (x.Headers.TryGetValue(key, out var value))
                    return [Encoding.UTF8.GetString((byte[])value)];
                return [];
            });
            Baggage.Current = parent.Baggage;

            return parent;
        }

        public static void AddTagsToActivity(Activity activity, string exchangeName, string routingKey, byte[] body)
        {
            if (activity == null) return;

            activity.SetTag("message", Encoding.UTF8.GetString(body));
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination", exchangeName);
            activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
        }

        #endregion
    }
}
