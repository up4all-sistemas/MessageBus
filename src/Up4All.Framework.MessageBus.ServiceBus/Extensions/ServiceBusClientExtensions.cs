
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class ServiceBusClientExtensions
    {

        public static (ServiceBusClient, ServiceBusSender) CreateClient(ILogger logger, string connectionString, string entityName, int attempts)
        {
            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    logger.LogInformation("Failed to create connect in ServiceBus server, retrying in {Wait}", wait);
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    logger.LogDebug("Creating connection to ServiceBus server");
                    var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions { Identifier = $"up4-{Environment.MachineName.ToLower()}" });
                    var queueClient = client.CreateSender(entityName);
                    return (client, queueClient);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusClient CreateClient(string connectionString, int attempts)
        {
            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    return new ServiceBusClient(connectionString, new ServiceBusClientOptions { Identifier = $"up4-{Environment.MachineName.ToLower()}" });
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusMessage PrepareMesssage(MessageBusMessage message)
        {
            message.AddUserProperty("mb-timestamp", DateTime.UtcNow.ToString("o"));
            message.AddUserProperty("mb-messagebus", "servicebus");
            message.AddUserProperty("mb-id", Guid.NewGuid().ToString());

            var sbMessage = new ServiceBusMessage(message.Body);
            if (message.UserProperties.Any())
                foreach (var prop in message.UserProperties)
                    sbMessage.ApplicationProperties.Add(prop.Key, prop.Value);

            if (message.IsJson)
                sbMessage.ContentType = "application/json";

            return sbMessage;
        }

        public static ServiceBusMessage PrepareMesssage<TModel>(TModel message)
        {
            var sbMessage = new ServiceBusMessage
            {
                Body = BinaryData.FromString(JsonSerializer.Serialize(message.CreateMessagebusMessage(), new JsonSerializerOptions(JsonSerializerDefaults.Web))),
                ContentType = "application/json"
            };

            sbMessage.ApplicationProperties.Add("mb-timestamp", DateTime.UtcNow.ToString("o"));
            sbMessage.ApplicationProperties.Add("mb-messagebus", "servicebus");
            sbMessage.ApplicationProperties.Add("mb-id", Guid.NewGuid().ToString());

            return sbMessage;
        }

        public static Task RegisterHandleMessageAsync(this ServiceBusProcessor client, ILogger logger, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Registrating Deliver Consumer to {EntityPath}", client.EntityPath);
            client.ProcessMessageAsync += async (arg) => await ProcessMessageAsync(logger, arg, handler, errorHandler, DefineIdleHandler(onIdle), autoComplete, cancellationToken);
            client.ProcessErrorAsync += async (ex) => await ProcessOnError(ex.Exception, errorHandler, cancellationToken);

            return Task.CompletedTask;
        }

        public static Task SendMessageBusMessageAsync(this ServiceBusSender sender, ILogger logger, MessageBusMessage message, CancellationToken cancellationToken)
        {
            logger.LogDebug("Sending message to {EntityPath}", sender.EntityPath);
            return sender.SendMessageAsync(PrepareMesssage(message), cancellationToken);
        }

        public static Task SendMessageBusMessageAsync(this ServiceBusSender sender, ILogger logger, IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken)
        {
            logger.LogDebug("Sending messages to {EntityPath}", sender.EntityPath);
            return sender.SendMessagesAsync(messages.Select(PrepareMesssage), cancellationToken);
        }

        private static async Task ProcessMessageAsync(ILogger logger, ProcessMessageEventArgs arg
            , Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler
            , Func<Exception, CancellationToken, Task> onError
            , Func<CancellationToken, Task> onIdle
            , bool autoComplete
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);

            logger.LogDebug("Receiving message from {EntityPath}", arg.EntityPath);

            var received = CreateMessage(arg);

            try
            {
                await ProcessHandleResult(arg, await handler.Invoke(received, cancellationToken), autoComplete, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "", ex.Message);
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);
                await onError.Invoke(ex, cancellationToken);
            }

            await onIdle.Invoke(cancellationToken);
        }

        private static Func<CancellationToken, Task> DefineIdleHandler(Func<CancellationToken, Task> onIdle = null)
        {
            return onIdle ?? ProcessOnIdle;
        }

        private static async Task ProcessHandleResult(ProcessMessageEventArgs arg, MessageReceivedStatus result, bool autoComplete, CancellationToken cancellationToken)
        {
            if (result == MessageReceivedStatus.Deadletter)
                await arg.DeadLetterMessageAsync(arg.Message);

            if (result == MessageReceivedStatus.Abandoned)
                await arg.AbandonMessageAsync(arg.Message);

            if (!autoComplete)
                await arg.CompleteMessageAsync(arg.Message, cancellationToken);
        }

        private static Task ProcessOnIdle(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        private static async Task ProcessOnError(Exception ex, Func<Exception, CancellationToken, Task> onError, CancellationToken cancellationToken)
        {
            await onError(ex, cancellationToken);
        }

        private static ReceivedMessage CreateMessage(ProcessMessageEventArgs arg)
        {
            var received = new ReceivedMessage();

            if (arg.Message.ContentType == "application/json")
                received.AddBody(arg.Message.Body, true);
            else
                received.AddBody(arg.Message.Body.ToArray());

            if (arg.Message.ApplicationProperties.Any())
                received.AddUserProperties(arg.Message.ApplicationProperties.ToDictionary(x => x.Key, x => x.Value));

            return received;
        }
    }




}
