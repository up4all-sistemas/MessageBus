# Up4All.Framework.MessageBus.TransferHelper

Cross-broker message transfer (shovel/forward) helper for [Up4All.MessageBus](https://github.com/up4all-sistemas/Up4All.MessageBus) — consumes from one broker/entity and republishes each message to another, using two `Up4All.Framework.MessageBus.Abstractions` pipelines (source and destination) that don't have to be the same broker type. Built on top of `Up4All.Framework.MessageBus.Abstractions`; you still add a concrete provider package (RabbitMQ, Service Bus, Kafka) for whichever brokers you're transferring between.

## What it does

- **Source → destination wiring** — consumes from a source pipeline (queue/topic/stream, any provider) and publishes each message to a destination pipeline (any provider), so you can shovel messages between RabbitMQ and Service Bus, Kafka and RabbitMQ, or even two entities on the same broker.
- **`ConsumerHandler<TOptionsSource, TOptionsDest>`** — the `IMessageBusMessageHandler` that drives the transfer: runs `IBeforeTransferHandler.CanTransfer` (skip filter), `ITransformationHandler.TransformAsync` (message rewrite), `IBeforeTransferHandler.OnBeforeTransfer` (last-chance hook), then publishes to the destination, stamping `mb-transfer-from` and `mb-transfer-timestamp` on the outgoing message.
- **Header transformations declared in configuration** — add, remove, update or rename (`change-key`) user properties on the way through, via `TransferOptions.Transformations`, with no code required for the common cases.
- **Pluggable hooks** — override `IBeforeTransferHandler` to filter or mutate messages just before they're republished, and `ITransformationHandler` to implement transformations beyond what the declarative config covers (e.g. body rewriting).
- **`TransferOptions<TSourceOptions, TDestinationOptions>`** — binds `Source`/`Destination` broker options (each a provider's own `MessageBusOptions` subtype) plus `Transformations` from a single configuration section.
- **`MessageBusTransferHostedService`** — an `IHostedService` that starts/stops an `IMessageBusTransferClient`, useful when the transfer runs as the sole job of a worker process (calls `IHostApplicationLifetime.StopApplication()` once stopped).
- **Built-in OpenTelemetry tracing** for the transfer span, propagating context extracted from the source message.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.TransferHelper --version <latest-version>
```

This pulls in `Up4All.Framework.MessageBus.Abstractions` transitively. Add the provider package(s) for whichever broker(s) you're transferring between (e.g. `Up4All.Framework.MessageBus.RabbitMQ` and `Up4All.Framework.MessageBus.ServiceBus`).

## Configuration

```json
{
  "MessageBusTransferOptions": {
    "Source": {
      "ConnectionString": "amqp://guest:guest@localhost:5672",
      "QueueName": "orders-queue"
    },
    "Destination": {
      "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...",
      "TopicName": "orders-topic"
    },
    "Transformations": {
      "InHeader": [
        { "Key": "internal-flag", "Operation": "remove" },
        { "Key": "schema-version", "Operation": "add", "Value": "2.0" },
        { "Key": "customer-id", "Operation": "change-key", "Value": "customer-external-id" }
      ]
    }
  }
}
```

`Operation` accepts `"add"`, `"remove"`, `"update"` and `"change-key"` (renames the property, keeping its value). `Source`/`Destination` are each a provider-specific options type (e.g. `RabbitMQMessageBusOptions`, `MessageBusOptions` for Service Bus/Kafka) bound from the matching sub-section — declare only the fields that broker needs (`QueueName`, `TopicName`/`SubscriptionName`, `StreamName`, ...).

## Getting started

```csharp
// Program.cs
builder.Services.AddTransfer<RabbitMQMessageBusOptions, MessageBusOptions>() // binds "MessageBusTransferOptions" by default
    .AddSource(() => builder.Services.AddRabbitMQMessageBus("MessageBusTransferOptions:Source")
        .Queues.ListenQueue())
    .AddDestination(() => builder.Services.AddServiceBusMessageBus("MessageBusTransferOptions:Destination")
        .Producers.AddPublisher());

builder.Services.AddHostedService<MessageBusTransferHostedService>();
```

`AddTransfer<TSourceOptions, TDestinationOptions>` returns a `TransferPipeline` that:

- `AddSource(...)` registers the consumer side: it wires `ConsumerHandler<TSourceOptions, TDestinationOptions>` as the handler on whatever `IConsumerPipelineBuilder` you built (queue/topic/stream, any provider), calls `AddDefaultHostedService()` on it, and registers the default (pass-through) `IBeforeTransferHandler`/`ITransformationHandler`.
- `AddDestination(...)` registers the publisher side — any `IPublishPipelineBuilder` from a provider.
- `Validate()` (called automatically where providers wire it up, or manually) throws if either side wasn't configured.

A message arriving on the source queue is deserialized into a `ReceivedMessage`, run through the transfer pipeline, and republished as a `MessageBusMessage` on the destination — with `mb-transfer-from` (source entity path) and `mb-transfer-timestamp` added automatically.

## Customizing the transfer

### Filtering and last-chance mutation

```csharp
public class OrdersBeforeTransferHandler(ILogger<OrdersBeforeTransferHandler> logger)
    : DefaultBeforeTransferHandler(logger)
{
    public override Task<bool> CanTransfer(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
    {
        // skip test messages instead of forwarding them
        return Task.FromResult(!receivedMessage.GetUserPropertyValueAsString("env")?.Equals("test") ?? true);
    }

    public override Task OnBeforeTransfer(MessageBusMessage destinationMessage, CancellationToken cancellationToken)
    {
        destinationMessage.AddUserProperty("forwarded-by", "orders-shovel");
        return base.OnBeforeTransfer(destinationMessage, cancellationToken);
    }
}
```

```csharp
pipeline.AddBeforeTransferHandler<OrdersBeforeTransferHandler>();
```

`CanTransfer` returning `false` skips the message entirely (it's neither transformed nor published — just logged and dropped from the transfer, though it's still settled as `Completed` on the source). `OnBeforeTransfer` runs after transformation, right before publishing, and is the place to add/inspect properties that depend on the already-transformed message.

### Transformations beyond configuration

```csharp
public class OrdersTransformationHandler(ILogger<OrdersTransformationHandler> logger)
    : DefaultTransformationHandler(logger)
{
    public override async Task<MessageBusMessage> TransformAsync(ReceivedMessage receivedMessage, TransferTransformations? transformationOptions, CancellationToken cancellationToken)
    {
        var message = await base.TransformAsync(receivedMessage, transformationOptions, cancellationToken); // applies InHeader rules
        // rewrite the body, e.g. mapping between two schemas
        return message;
    }
}
```

```csharp
pipeline.AddTransferTransformationHandler<OrdersTransformationHandler>();
```

The default `ITransformationHandler` only applies `Transformations.InHeader` rules; `Transformations.InBody` is reserved in `TransferOptions` for callers to interpret in their own handler — there's no built-in body transformation logic.

## Standalone transfer (no hosting)

Implement `IMessageBusTransferClient` (`StartAsync`/`StopAsync`) around your own consumer/publisher wiring and drive it directly, or register it and let `MessageBusTransferHostedService` start/stop it as part of the host lifecycle — the hosted service also calls `IHostApplicationLifetime.StopApplication()` once stopped, which is convenient when the transfer is the only job the process does (it shuts the whole process down once the transfer client stops).

## OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOpenTelemetryForTransferBusMessageBus());
```

This registers the library's `ActivitySource`. `ConsumerHandler` extracts the trace context carried in the source message's user properties and creates a `message-transfer` span as a child of it, tagged with `messaging.system = "messagebus-transfer"` and the usual messaging semantic-convention attributes — so a transfer shows up as a continuation of the original publisher's trace, not a disconnected one. As with the provider packages, propagation goes through the process-wide `OpenTelemetry.Context.Propagation.Propagators.DefaultTextMapPropagator`, which this library doesn't configure itself; set one explicitly during startup if nothing else in your application already does (see the provider READMEs' OpenTelemetry section).

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see [`license.txt`](./license.txt) for the full text.
