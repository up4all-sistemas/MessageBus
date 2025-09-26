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
using System.Threading.Channels;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class RabbitMQClientExtensions
    {
        private static readonly ActivitySource activitySource = new(Consts.OpenTelemetrySourceName);
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public static void ConfigureAsyncHandler(this IRabbitMQClient client, string queueName, AsyncQueueMessageReceiver receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void ConfigureAsyncHandler<TModel>(this IRabbitMQClient client, string queueName, AsyncQueueMessageReceiverForModel<TModel> receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void ConfigureHandler(this IRabbitMQClient client, string queueName, QueueMessageReceiver receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void ConfigureHandler<TModel>(this IRabbitMQClient client, string queueName, QueueMessageReceiverForModel<TModel> receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add(Arguments.StreamOffsetKey, offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void SendMessage(this IModel channel, string topicName, string queueName, MessageBusMessage msg)
        {
            var activityName = $"message-send {topicName} {queueName}";
            var basicProps = channel.CreateBasicProperties();
            basicProps.PopulateHeaders(msg);

            using var activity = ProcessOpenTelemetryActivity(activityName, ActivityKind.Producer);
            var routingKey = queueName;

            if (msg.ContainsRoutingKey())
                routingKey = msg.GetRoutingKey();

            InjectPropagationContext(activity, basicProps);
            AddTagsToActivity(activity, topicName, routingKey, msg.Body);

            channel.BasicPublish(topicName, routingKey, basicProps, msg.Body);

        }

        public static IConnection GetConnection(this IRabbitMQClient client, MessageBusOptions opts, ILogger<IRabbitMQClient> logger)
        {
            if (client.Connection != null) return client.Connection;

            IConnection conn = null;
            var result = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetry(opts.ConnectionAttempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    logger.LogInformation("Failed to connect in RabbitMQ server, retrying in {Wait}", wait);
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    logger.LogDebug("Trying to connect in RabbitMQ server");
                    conn = new ConnectionFactory() { Uri = new Uri(opts.ConnectionString) }.CreateConnection();
                });

            if (result.Outcome != OutcomeType.Successful)
                throw result.FinalException;

            client.Connection = conn;
            return conn;
        }

        public static IConnection GetConnection(this IRabbitMQClient client, string connectionString, int connectionAttempts, bool isAsyncConsumer = false)
        {
            if (client.Connection != null) return client.Connection;

            IConnection conn = null;
            var result = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetry(connectionAttempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    if (conn != null && conn.IsOpen) return;
                    conn = new ConnectionFactory() { Uri = new Uri(connectionString), DispatchConsumersAsync = isAsyncConsumer }.CreateConnection();
                });

            if (result.Outcome != OutcomeType.Successful)
                throw result.FinalException;

            client.Connection = conn;
            return conn;
        }

        public static IModel CreateChannel(this IRabbitMQClient client)
        {
            var channel = client.Connection.CreateModel();
            return channel;
        }

        public static void ConfigureQueueDeclare(this IModel channel, string queueName, QueueDeclareOptions declareOpts)
        {
            if (declareOpts == null) return;

            if (!declareOpts.Args.ContainsKey("x-queue-type"))
                declareOpts.Args.Add("x-queue-type", declareOpts.Type);

            channel.QueueDeclare(queue: queueName, durable: declareOpts.Durable, exclusive: declareOpts.Exclusive, autoDelete: declareOpts.AutoDelete, arguments: declareOpts.Args);

            if (declareOpts.Bindings.Any())
                foreach (var bind in declareOpts.Bindings)
                    channel.QueueBind(queueName, bind.ExchangeName, bind.RoutingKey ?? "", bind.Args);

        }

        public static string CreateActivityName(this IBasicConsumer consumer, string activityName, string exchangeName, string routingKey)
        {
            return $"{activityName} {exchangeName} {routingKey} {consumer.Model.CurrentQueue}";
        }

        public static string CreateMessageReceivedActivityName(this IBasicConsumer consumer, string exchangeName, string routingKey)
        {
            return consumer.CreateActivityName("message-received", exchangeName, routingKey);    
        }

        public static Activity ProcessOpenTelemetryActivity(string activityName, ActivityKind kind, ActivityContext parent = default)
        {
            var activity = activitySource.StartActivity(activityName, kind, parent);
            return activity;
        }

        public static Activity CreateActivity(this IBasicProperties properties, string activityName, ActivityKind kind)
        {
            var parentContext = GetParentPropagationContext(properties);
            var activity = ProcessOpenTelemetryActivity(activityName, kind, parentContext.ActivityContext);
            return activity;
        }

        public static Activity CreateMessageReceivedActivity(this IBasicConsumer consumer, IBasicProperties properties, string exchangeName, string routingKey)
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

        public static PropagationContext GetParentPropagationContext(IBasicProperties props)
        {
            var parent = Propagator.Extract(default, props, (x, key) =>
            {
                if (x.Headers.TryGetValue(key, out var value))
                    return [ Encoding.UTF8.GetString((byte[])value) ];
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

        public static void ProcessMessage(this IModel channel, ulong deliveryTag, MessageReceivedStatus status, bool autoComplete)
        {
            if (!autoComplete && status == MessageReceivedStatus.Deadletter)
            {
                channel.BasicNack(deliveryTag, false, false);
                return;
            }

            if (!autoComplete && status == MessageReceivedStatus.Abandoned)
            {
                channel.BasicReject(deliveryTag, true);
                return;
            }

            if (!autoComplete)
                channel.BasicAck(deliveryTag, false);
        }

        public static void ProcessErrorMessage(this IModel _channel, ulong deliveryTag, bool autoComplete)
        {
            if (!autoComplete)
                _channel.BasicNack(deliveryTag, false, false);
        }
    }
}
