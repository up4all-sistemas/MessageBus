using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Attributes;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;

namespace Up4All.Framework.MessageBus.Tests.Support
{
    // ---- Sample POCOs used by MessageBusExtensions.CreateMessagebusMessage tests ----

    [MessageBusPayload("my-target")]
    [MessageBusRoutingKey("my-routing-key")]
    [MessageBusAdditionalUserProperty("extra-key", "extra-value")]
    [MessageBusAdditionalUserProperty("extra-key-2", 42)]
    public class SampleModel
    {
        public string Name { get; set; } = string.Empty;

        [MessageBusUserProperty("custom-prop")]
        public int Code { get; set; }
    }

    public class PlainModel
    {
        public string Value { get; set; } = string.Empty;
    }

    public class IdPayload
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // ---- Standalone client test doubles ----

    public class FakeStandaloneQueueClient(string connectionString, string queueName, int connectionAttempts = 8)
        : MessageBusStandaloneQueueClient(connectionString, queueName, connectionAttempts)
    {
        public bool DisposeCalled { get; private set; }
        public bool LastDisposing { get; private set; }

        public string ExposedConnectionString => ConnectionString;
        public int ExposedConnectionAttempts => ConnectionAttempts;

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            LastDisposing = disposing;
        }
    }

    public class FakeStandaloneStreamClient(string connectionString, string queueName, object offset, int connectionAttempts = 8)
        : MessageBusStandaloneStreamClient(connectionString, queueName, offset, connectionAttempts)
    {
        public bool DisposeCalled { get; private set; }
        public bool LastDisposing { get; private set; }

        public string ExposedConnectionString => ConnectionString;
        public int ExposedConnectionAttempts => ConnectionAttempts;
        public object ExposedOffset => Offset;

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            LastDisposing = disposing;
        }
    }

    public class FakeStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8)
        : MessageBusStandaloneTopicClient(connectionString, topicName, connectionAttempts)
    {
        public bool DisposeCalled { get; private set; }
        public bool LastDisposing { get; private set; }

        public string ExposedConnectionString => ConnectionString;
        public int ExposedConnectionAttempts => ConnectionAttempts;

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            LastDisposing = disposing;
        }
    }

    public class FakeStandaloneSubscriptonClient(string connectionString, string topicName, string subscriptionName)
        : MessageBusStandaloneSubscriptonClient(connectionString, topicName, subscriptionName)
    {
        public bool DisposeCalled { get; private set; }
        public bool LastDisposing { get; private set; }

        public string ExposedConnectionString => ConnectionString;
        public string ExposedTopicName => TopicName;

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            LastDisposing = disposing;
        }
    }

    // ---- DefaultConsumer test doubles ----

    public class FakeAsyncConsumer : IMessageBusAsyncConsumer
    {
        public string EntityPath { get; set; } = "fake-entity";

        public Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>>? CapturedHandler { get; private set; }
        public Func<Exception, CancellationToken, Task>? CapturedErrorHandler { get; private set; }
        public bool AutoCompleteReceived { get; private set; }
        public CancellationToken RegisterCancellationToken { get; private set; }

        public bool CloseCalled { get; private set; }
        public CancellationToken CloseCancellationToken { get; private set; }

        public Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            CapturedHandler = handler;
            CapturedErrorHandler = errorHandler;
            AutoCompleteReceived = autoComplete;
            RegisterCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            CloseCalled = true;
            CloseCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    public class FakeMessageHandler : IMessageBusMessageHandler
    {
        public List<(string EntityPath, ReceivedMessage Message)> ReceivedCalls { get; } = [];
        public List<Exception> Errors { get; } = [];
        public bool ThrowOnReceive { get; set; }

        public Task OnMessageReceivedAsync(string entityPath, ReceivedMessage message, CancellationToken cancellationToken)
        {
            ReceivedCalls.Add((entityPath, message));
            if (ThrowOnReceive)
                throw new InvalidOperationException("boom");

            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            Errors.Add(exception);
            return Task.CompletedTask;
        }
    }

    // ---- Pipeline test doubles ----

    public class FakePipeline(IServiceCollection services, string configurationBindKey)
        : MessageBusPipeline<MessageBusOptions>(services, configurationBindKey)
    {
        public void CallAddOptions() => AddOptions();

        public void AddInnerPipeline(IPipelineBuilder pipeline) => _pipelines.Add(pipeline);
    }

    public class FakeInnerPipelineBuilder : IPipelineBuilder
    {
        public bool ValidateCalled { get; private set; }

        public void Validate() => ValidateCalled = true;
    }

    public class FakeInnerHandlerPipelineBuilder : IHandlerPipelineBuilder
    {
        public bool ValidateCalled { get; private set; }

        public void Validate() => ValidateCalled = true;

        public IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>()
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
            => this;

        public IHandlerPipelineBuilder AddHandler<TMessageBusMessageHandler>(Func<IServiceProvider, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
            => this;

        public IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object serviceKey)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
            => this;

        public IHandlerPipelineBuilder AddKeyedHandler<TMessageBusMessageHandler>(object servicekey, Func<IServiceProvider, object?, TMessageBusMessageHandler> builder)
            where TMessageBusMessageHandler : class, IMessageBusMessageHandler
            => this;
    }

    public class FakePublisherPipeline(FakePipeline main)
        : MessageBusPublisherPipeline<FakePipeline, MessageBusOptions>(main)
    {
        public void MarkPublisherDefined() => IsPublisherDefined = true;
    }

    public class FakeConsumerPipeline(FakePipeline main)
        : MessageBusConsumerPipeline<FakePipeline>(main)
    {
        public IHandlerPipelineBuilder CallAddHandlerPipeline(IHandlerPipelineBuilder handlerPipeline)
            => AddHandlerPipeline(handlerPipeline);

        public IReadOnlyList<IHandlerPipelineBuilder> ExposedHandlers => Handlers;
    }

    public class FakeHandlerPipeline(FakePipeline main)
        : MessageBusHandlerPipeline<FakePipeline, MessageBusOptions>(main)
    {
        public IHandlerPipelineBuilder CallAddKeyedHostedService<THandler>(object serviceKey)
            where THandler : notnull, IMessageBusMessageHandler
            => AddKeyedHostedService<THandler>(serviceKey);
    }
}
