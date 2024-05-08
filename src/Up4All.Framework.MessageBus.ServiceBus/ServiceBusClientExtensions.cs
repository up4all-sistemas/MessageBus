
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

        public static (ServiceBusClient, ServiceBusSender) CreateClient(MessageBusOptions opts, bool isTopicClient = false)
        {
            var entitypath = opts.QueueName;

            if (isTopicClient)
                entitypath = opts.TopicName;

            return CreateClient(opts.ConnectionString, entitypath, opts.ConnectionAttempts);
        }

        public static (ServiceBusClient, ServiceBusSender) CreateClient(string connectionString, string entityName, int attempts)
        {
            var logger = CreateLogger<IServiceBusClient>();

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
                    var client = new ServiceBusClient(connectionString);
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
                    return new ServiceBusClient(connectionString);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusMessage PrepareMesssage(MessageBusMessage message)
        {
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
                Body = BinaryData.FromString(JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web))),
                ContentType = "application/json"
            };
            return sbMessage;
        }

        public static Task RegisterHandleMessageAsync(this ServiceBusProcessor client, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            client.ProcessMessageAsync += async (arg) => await ProcessMessageAsync(arg, handler, DefineIdleHandler(onIdle), autoComplete, cancellationToken);
            client.ProcessErrorAsync += async (ex) => await ProcessOnError(ex.Exception, errorHandler, cancellationToken);

            return Task.CompletedTask;
        }

        public static Task RegisterHandleMessageAsync<TModel>(this ServiceBusProcessor client, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {


            client.ProcessMessageAsync += async (arg) => await ProcessMessageAsync(arg, handler, DefineIdleHandler(onIdle), autoComplete, cancellationToken);
            client.ProcessErrorAsync += async (ex) => await ProcessOnError(ex.Exception, errorHandler, cancellationToken);

            return Task.CompletedTask;
        }

        public static void RegisterHandleMessage(this ServiceBusProcessor client, Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
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

        public static void RegisterHandleMessage<TModel>(this ServiceBusProcessor client, Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
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

        private static async Task ProcessMessageAsync<TModel>(ProcessMessageEventArgs arg
            , Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler
            , Func<CancellationToken, Task> onIdle
            , bool autoComplete
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);

            var received = CreateMessage(arg);

            try
            {
                var model = received.GetBody<TModel>();
                await ProcessHandleResult(arg, await handler(model, cancellationToken), autoComplete, cancellationToken);
            }
            catch (Exception)
            {
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);
                throw;
            }

            await onIdle.Invoke(cancellationToken);
        }

        private static async Task ProcessMessageAsync(ProcessMessageEventArgs arg
            , Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler
            , Func<CancellationToken, Task> onIdle
            , bool autoComplete
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);

            var received = CreateMessage(arg);

            try
            {
                await ProcessHandleResult(arg, await handler(received, cancellationToken), autoComplete, cancellationToken);
            }
            catch (Exception)
            {
                await arg.AbandonMessageAsync(arg.Message, cancellationToken: cancellationToken);
                throw;
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
