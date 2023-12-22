using Microsoft.Extensions.Logging;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class RabbitMQClientExtensions
    {
        public static void ConfigureHandler(this IRabbitMQClient client, IModel channel, string queueName, QueueMessageReceiver receiver, bool autoComplete, object offset = null)
        {
            channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add("x-stream-offset", offset);

            channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
        }

        public static void ConfigureHandler<TModel>(this IRabbitMQClient client, IModel channel, string queueName, QueueMessageReceiverForModel<TModel> receiver, bool autoComplete, object offset = null)
        {
            channel.BasicQos(0, 1, false);

            var args = new Dictionary<string, object> { };
            if (offset != null) args.Add("x-stream-offset", offset);

            channel.BasicConsume(queue: queueName, autoAck: autoComplete, consumer: receiver, arguments: args);
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
                    logger.LogInformation($"Failed to connect in RabbitMQ server, retrying in {wait}");
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    logger.LogDebug($"Trying to connect in RabbitMQ server");
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
        public static IModel CreateChannel(this IRabbitMQClient client, IConnection conn)
        {
            return conn.CreateModel();
        }
    }
}
