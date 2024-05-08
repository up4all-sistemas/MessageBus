using Microsoft.Extensions.Logging;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class RabbitMQClientExtensions
    {
        public static void ConfigureHandler(this IRabbitMQClient client, string queueName, QueueMessageReceiver receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add("x-stream-offset", offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void ConfigureHandler<TModel>(this IRabbitMQClient client, string queueName, QueueMessageReceiverForModel<TModel> receiver, bool autoComplete, object offset = null)
        {
            client.Channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add("x-stream-offset", offset);

            client.Channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static MessageBusMessage CreateMessagebusMessage<TModel>(this TModel model)
        {
            var message = new MessageBusMessage();
            message.AddBody(BinaryData.FromString(JsonSerializer.Serialize(model, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
            return message;
        }

        public static void SendMessage(this IModel channel, string topicName, string queueName, MessageBusMessage msg, CancellationToken cancellationToken)
        {
            var basicProps = channel.CreateBasicProperties();
            basicProps.PopulateHeaders(msg);

            var routingKey = queueName;

            if (msg.UserProperties.ContainsKey("routing-key"))
                routingKey = msg.UserProperties["routing-key"].ToString();

            cancellationToken.ThrowIfCancellationRequested();
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

        public static IConnection GetConnection(this IRabbitMQClient client, string connectionString, int connectionAttempts)
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
                    conn = new ConnectionFactory() { Uri = new Uri(connectionString) }.CreateConnection();
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
    }
}
