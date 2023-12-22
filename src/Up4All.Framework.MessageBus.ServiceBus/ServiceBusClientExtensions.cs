
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public static class ServiceBusClientExtensions
    {

        public static (ServiceBusClient, ServiceBusSender) CreateClient(this IServiceBusClient sbclient, MessageBusOptions opts, bool isTopicClient = false)
        {
            var entitypath = opts.QueueName;

            if (isTopicClient)
                entitypath = opts.TopicName;

            return CreateClient(sbclient, opts.ConnectionString, entitypath, opts.ConnectionAttempts);
        }

        public static (ServiceBusClient, ServiceBusSender) CreateClient(this IServiceBusClient sbclient, string connectionString, string entityName, int attempts)
        {
            var logger = CreateLogger<IServiceBusClient>();

            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    logger.LogInformation($"Failed to create connect in ServiceBus server, retrying in {wait}");
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    logger.LogDebug($"Creating connection to ServiceBus server");
                    var client = new ServiceBusClient(connectionString);
                    var queueClient = client.CreateSender(entityName);
                    return (client, queueClient);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusClient CreateClient(this IServiceBusClient sbclient, string connectionString, int attempts)
        {
            var logger = CreateLogger<IServiceBusClient>();

            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    return new ServiceBusClient(connectionString);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusMessage PrepareMesssage(this IServiceBusClient client, MessageBusMessage message)
        {
            var sbMessage = new ServiceBusMessage(message.Body);
            if (message.UserProperties.Any())
                foreach (var prop in message.UserProperties)
                    sbMessage.ApplicationProperties.Add(prop.Key, prop.Value);

            if (message.IsJson)
                sbMessage.ContentType = "application/json";

            return sbMessage;
        }

        public static ServiceBusMessage PrepareMesssage<TModel>(this IServiceBusClient client, TModel message)
        {
            var sbMessage = new ServiceBusMessage();
            sbMessage.Body = BinaryData.FromString(JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            sbMessage.ContentType = "application/json";
            return sbMessage;
        }

        public static Task RegisterHandleMessageAsync(this ServiceBusProcessor client, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            client.ProcessMessageAsync += async (arg) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);

                var received = new ReceivedMessage();

                if (arg.Message.ContentType == "application/json")
                    received.AddBody(arg.Message.Body, true);
                else
                    received.AddBody(arg.Message.Body.ToArray());

                if (arg.Message.ApplicationProperties.Any())
                    received.AddUserProperties(arg.Message.ApplicationProperties.ToDictionary(x => x.Key, x => x.Value.ToString()));

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = await handler(received, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (result == MessageReceivedStatusEnum.Deadletter)
                        await arg.DeadLetterMessageAsync(arg.Message);
                    else if (result == MessageReceivedStatusEnum.Abandoned)
                        await arg.AbandonMessageAsync(arg.Message);

                    if (!autoComplete) await arg.CompleteMessageAsync(arg.Message, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);
                    throw;
                }

                await onIdle?.Invoke(cancellationToken);
            };

            client.ProcessErrorAsync += async (ex) =>
            {
                await errorHandler(ex.Exception, cancellationToken);
            };

            return Task.CompletedTask;
        }

        public static Task RegisterHandleMessageAsync<TModel>(this ServiceBusProcessor client, Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            client.ProcessMessageAsync += async (arg) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);

                var received = new ReceivedMessage();

                if (arg.Message.ContentType == "application/json")
                    received.AddBody(arg.Message.Body, true);
                else
                    received.AddBody(arg.Message.Body.ToArray());

                if (arg.Message.ApplicationProperties.Any())
                    received.AddUserProperties(arg.Message.ApplicationProperties.ToDictionary(x => x.Key, x => x.Value.ToString()));

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var model = received.GetBody<TModel>();
                    var result = await handler(model, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (result == MessageReceivedStatusEnum.Deadletter)
                        await arg.DeadLetterMessageAsync(arg.Message);
                    else if (result == MessageReceivedStatusEnum.Abandoned)
                        await arg.AbandonMessageAsync(arg.Message);

                    if (!autoComplete) await arg.CompleteMessageAsync(arg.Message, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);
                    throw;
                }

                await onIdle?.Invoke(cancellationToken);
            };

            client.ProcessErrorAsync += async (ex) =>
            {
                await errorHandler(ex.Exception, cancellationToken);
            };

            return Task.CompletedTask;
        }

        public static void RegisterHandleMessage(this ServiceBusProcessor client, Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            client.RegisterHandleMessageAsync((msg, token) =>
            {
                return Task.FromResult(handler(msg));
            }, (ex, token) =>
            {
                errorHandler.Invoke(ex);
                return Task.CompletedTask;
            }, (token) =>
            {
                onIdle.Invoke();
                return Task.CompletedTask;
            }, autoComplete);
        }

        public static void RegisterHandleMessage<TModel>(this ServiceBusProcessor client, Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            client.RegisterHandleMessageAsync((msg, token) =>
            {
                var model = msg.GetBody<TModel>();
                return Task.FromResult(handler(model));
            }, (ex, token) =>
            {
                errorHandler.Invoke(ex);
                return Task.CompletedTask;
            }, (token) =>
            {
                onIdle.Invoke();
                return Task.CompletedTask;
            }, autoComplete);
        }

        private static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory
                    .Create(cfg => { })
                    .CreateLogger<T>();
        }
    }




}
