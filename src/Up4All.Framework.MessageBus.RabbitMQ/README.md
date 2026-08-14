# Up4All.Framework.MessageBus.RabbitMQ

RabbitMQ provider for [Up4All.MessageBus](https://github.com/up4all-sistemas/Up4All.MessageBus) — a lightweight abstraction for publishing and consuming messages that lets application code depend only on `Up4All.Framework.MessageBus.Abstractions`, while this package wires it up to RabbitMQ (queues, exchanges/topics and streams) using [RabbitMQ.Client](https://www.nuget.org/packages/RabbitMQ.Client) 7.x.

## What it does

- **Queues, topics/exchanges and streams** — publish/consume classic & quorum queues, publish/subscribe over exchanges, and consume RabbitMQ streams with offset control.
- **Fluent pipeline builder** registered through `IServiceCollection`, binding connection and provisioning settings straight from configuration (`IOptions<RabbitMQMessageBusOptions>`).
- **Automatic provisioning** of queues, exchanges and bindings (declare-on-start), fully optional.
- **Background consumption** via a ready-made `IHostedService` (`AddDefaultHostedService()` / `AddDefaultStreamHostedService()`), or drive it yourself.
- **Standalone clients** for when you don't want DI/hosting at all — just a connection string and a queue/exchange/stream name. They implement both `IDisposable` and `IAsyncDisposable`, so `await using` closes the channel/connection asynchronously instead of blocking a thread.
- **Message attributes** (`[MessageBusPayload]`, `[MessageBusRoutingKey]`, `[MessageBusUserProperty]`, `[MessageBusAdditionalUserProperty]`) to map POCOs to AMQP metadata without hand-building `MessageBusMessage`.
- **Built-in OpenTelemetry tracing** for publish/consume spans, with W3C context propagated through AMQP headers.
- **Resilient connections** via Polly (exponential backoff on connect).
- **Keyed DI support** (`AddPublisher(key, ...)`, `ListenQueue(key, ...)`, `ListenStreamQueue(key, ...)`) for talking to more than one broker/entity from the same process.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.RabbitMQ --version <latest-version>
```

This pulls in `Up4All.Framework.MessageBus.Abstractions` transitively.

## Configuration

```json
{
  "MessageBusOptions": {
    "ConnectionString": "amqp://guest:guest@localhost:5672",
    "QueueName": "orders-queue",
    "TopicName": "orders-exchange",
    "StreamName": "orders-stream",
    "ConnectionAttempts": 8,
    "PersistentMessages": true,
    "ProvisioningOptions": {
      "Type": "classic",
      "Durable": true,
      "AutoDelete": false,
      "Bindings": [
        { "ExchangeName": "orders-exchange", "RoutingKey": "orders.created" }
      ]
    }
  }
}
```

`ProvisioningOptions` is optional — omit it if the queue/exchange already exists on the broker and you don't want this client declaring it.

## Getting started (dependency injection)

### Publishing

```csharp
// Program.cs
builder.Services.AddRabbitMQMessageBus() // binds the "MessageBusOptions" section by default
    .Producers.AddPublisher();           // publishes to MessageBusOptions:TopicName
```

```csharp
public class OrderService(IMessageBusPublisherAsync publisher)
{
    public Task NotifyOrderCreatedAsync(OrderCreated evt, CancellationToken ct)
        => publisher.SendAsync(evt, ct);
}
```

`SendAsync<TModel>` calls `model.CreateMessagebusMessage()` under the hood, serializing the payload as JSON and applying any `[MessageBus*]` attributes declared on `TModel` (see [Message attributes](#message-attributes)).

### Consuming a queue

```csharp
builder.Services.AddRabbitMQMessageBus()
    .Queues.ListenQueue()                 // binds to MessageBusOptions:QueueName
    .AddHandler<OrderCreatedHandler>()
    .AddDefaultHostedService();
```

```csharp
public class OrderCreatedHandler : IMessageBusMessageHandler
{
    public Task OnMessageReceivedAsync(string entityPath, ReceivedMessage message, CancellationToken cancellationToken)
    {
        var order = message.GetBody<OrderCreated>();
        // ... handle it
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        // log / alert
        return Task.CompletedTask;
    }
}
```

`AddDefaultHostedService()` registers a background `IHostedService` that calls `RegisterHandlerAsync` on startup; a message that throws inside `OnMessageReceivedAsync` is automatically abandoned (nacked) after `OnErrorAsync` runs — you don't ack/nack manually.

### Consuming a stream

Streams read from an offset (`OffsetType.First`, `.Last`, `.Next` — the default when omitted —, or a specific value):

```csharp
builder.Services.AddRabbitMQMessageBus()
    .Streams.ListenStreamQueue(offset: OffsetType.First) // binds to MessageBusOptions:StreamName
    .AddHandler<OrderCreatedHandler>()
    .AddDefaultStreamHostedService();
```

### Multiple connections (keyed services)

```csharp
pipeline.Queues.ListenQueue("orders-consumer", "amqp://localhost", "orders-queue");
pipeline.Producers.AddPublisher((object)"notifications-publisher", "amqp://otherhost", "notifications-exchange");
```

```csharp
public class OrdersWorker([FromKeyedServices("orders-consumer")] IMessageBusAsyncConsumer consumer) { /* ... */ }
```

## Standalone clients (no DI required)

```csharp
await using var client = new RabbitMQStandaloneQueueAsyncClient(logger, "amqp://guest:guest@localhost:5672", "orders-queue", persistent: true);

await client.SendAsync(new OrderCreated(Guid.NewGuid()));

await client.RegisterHandlerAsync(async (message, ct) =>
{
    var order = message.GetBody<OrderCreated>();
    return MessageReceivedStatus.Completed; // or Abandoned / Deadletter
}, (ex, ct) => Task.CompletedTask);
```

Equivalent standalone clients exist for exchanges (`RabbitMQStandaloneTopicAsyncClient`) and streams (`RabbitMQStandaloneStreamAsyncClient`).

## Message attributes

```csharp
[MessageBusPayload("orders-service")]
[MessageBusRoutingKey("orders.created")]
[MessageBusAdditionalUserProperty("schema-version", "1.0")]
public class OrderCreated
{
    public Guid OrderId { get; set; }

    [MessageBusUserProperty("customer-id")]
    public Guid CustomerId { get; set; }
}
```

Calling `orderCreated.CreateMessagebusMessage()` (used internally by `SendAsync<TModel>`) produces a `MessageBusMessage` with `target`/`routing-key`/`schema-version`/`customer-id` set as AMQP headers and the JSON payload as the body.

## Provisioning options

`ProvisioningOptions` (bound from configuration, or built in code) controls what gets declared on the broker before publishing/consuming:

```csharp
pipeline.Queues.ListenQueue((sp, options, declareOptions) =>
{
    declareOptions.AddBinding("orders-exchange", b => b.RoutingKey = "orders.created");
});
```

`declareOptions.Type` accepts `QueueType.Classic`, `QueueType.Quorum` or `QueueType.Stream`; `ExchangeType.Direct`, `.Fanout`, `.Topic` and `.Headers` are available for exchanges.

## OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOpenTelemetryForRabbitMQMessageBus());
```

This registers the library's `ActivitySource`. The library injects/extracts trace context into AMQP message headers on both publish and consume, but it does so through `System.Diagnostics.Activity` and `OpenTelemetry.Context.Propagation.Propagators.DefaultTextMapPropagator` — a **process-wide static** that the library deliberately does not configure itself, to avoid silently overriding a propagator (or its composition/order) that the hosting application may already have set up.

By default, `DefaultTextMapPropagator` is a no-op: trace context injection/extraction calls run without error but propagate nothing, so publish and consume spans end up disconnected (each consumed message starts a brand-new trace instead of continuing the publisher's). If nothing in your application already configures a propagator (e.g. via `AddAspNetCoreInstrumentation`/`AddHttpClientInstrumentation`, which typically do this as a side effect), set one explicitly during startup:

```csharp
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
    [new TraceContextPropagator(), new BaggagePropagator()]));
```

### Metrics

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddOpenTelemetryMetricsForRabbitMQMessageBus());
```

This registers the library's `Meter`, exposing three instruments that follow the OpenTelemetry messaging semantic conventions:

| Instrument | Type | Description |
|---|---|---|
| `messaging.client.sent.messages` | Counter | Messages successfully published. |
| `messaging.client.consumed.messages` | Counter | Messages the consumer finished processing (success or error - see `error.type`). |
| `messaging.client.operation.duration` | Histogram (seconds) | Duration of a publish or consume operation. |

All three carry `messaging.system`, `messaging.destination.name` and `messaging.operation.type` tags; failed operations additionally carry an `error.type` tag with the exception's type name.

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddRabbitMQMessageBusHealthCheck(sp => (IRabbitMQClient)sp.GetRequiredService<IMessageBusPublisherAsync>());
```

The check reads `IConnection.IsOpen` off whichever client the accessor resolves - it never opens a connection itself, so it stays cheap enough to poll frequently. The library can have several independent clients registered (e.g. multiple keyed `AddPublisher`/`AddConsumer` calls pointing at different brokers), so there is no single implicit "the" connection to default to - you tell it which one to check. Every concrete client (topic, queue, stream) implements `IRabbitMQClient`, so cast whichever one you registered.

Because the library connects lazily (on first publish/consume, not at DI resolution), a client that hasn't sent or received anything yet reports Unhealthy the same as one whose connection genuinely dropped - the two aren't distinguishable from `IConnection.IsOpen` alone.

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see [`license.txt`](./license.txt) for the full text.
