# Up4All.Framework.MessageBus.ServiceBus

Azure Service Bus provider for [Up4All.MessageBus](https://github.com/up4all-sistemas/Up4All.MessageBus) — a lightweight abstraction for publishing and consuming messages that lets application code depend only on `Up4All.Framework.MessageBus.Abstractions`, while this package wires it up to Azure Service Bus (queues and topics/subscriptions) using [Azure.Messaging.ServiceBus](https://www.nuget.org/packages/Azure.Messaging.ServiceBus) 7.x.

## What it does

- **Queues and topics/subscriptions** — publish/consume queues, and publish/subscribe over topics with named subscriptions.
- **Fluent pipeline builder** registered through `IServiceCollection`, binding connection settings straight from configuration (`IOptions<MessageBusOptions>`).
- **Background consumption** via a ready-made `IHostedService` (`AddDefaultHostedService()`), or drive it yourself.
- **Standalone clients** for when you don't want DI/hosting at all — just a connection string and a queue/topic/subscription name. They implement both `IDisposable` and `IAsyncDisposable`, so `await using` closes the underlying sender/processor asynchronously instead of blocking a thread.
- **Message attributes** (`[MessageBusPayload]`, `[MessageBusRoutingKey]`, `[MessageBusUserProperty]`, `[MessageBusAdditionalUserProperty]`) to map POCOs to Service Bus application properties without hand-building `MessageBusMessage`.
- **Built-in OpenTelemetry tracing** for publish/consume spans, with W3C context propagated through message application properties.
- **Resilient connections** via Polly (exponential backoff on connect).
- **Keyed DI support** (`AddPublisher(key, ...)`, `ListenQueue(key, ...)`, `ListenSubscription(key, ...)`) for talking to more than one namespace/entity from the same process.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.ServiceBus --version <latest-version>
```

This pulls in `Up4All.Framework.MessageBus.Abstractions` transitively.

## Configuration

```json
{
  "MessageBusOptions": {
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...",
    "QueueName": "orders-queue",
    "TopicName": "orders-topic",
    "SubscriptionName": "orders-subscription",
    "ConnectionAttempts": 8
  }
}
```

## Getting started (dependency injection)

### Publishing

```csharp
// Program.cs
builder.Services.AddServiceBusMessageBus() // binds the "MessageBusOptions" section by default
    .Producers.AddPublisher();             // publishes to MessageBusOptions:TopicName
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
builder.Services.AddServiceBusMessageBus()
    .Queues.ListenQueue()                  // binds to MessageBusOptions:QueueName
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

`AddDefaultHostedService()` registers a background `IHostedService` that calls `RegisterHandlerAsync` on startup; a message that throws inside `OnMessageReceivedAsync` is automatically abandoned after `OnErrorAsync` runs — you don't complete/abandon manually.

### Consuming a topic subscription

```csharp
builder.Services.AddServiceBusMessageBus()
    .Subscriptions.ListenSubscription()    // binds to MessageBusOptions:TopicName/SubscriptionName
    .AddHandler<OrderCreatedHandler>()
    .AddDefaultHostedService();
```

### Multiple connections (keyed services)

```csharp
pipeline.Queues.ListenQueue("orders-consumer", "Endpoint=sb://...", "orders-queue");
pipeline.Producers.AddPublisher((object)"notifications-publisher", "Endpoint=sb://other-namespace...", "notifications-topic");
```

```csharp
public class OrdersWorker([FromKeyedServices("orders-consumer")] IMessageBusAsyncConsumer consumer) { /* ... */ }
```

## Standalone clients (no DI required)

```csharp
await using var client = new ServiceBusStandaloneQueueAsyncClient(logger, "Endpoint=sb://your-namespace.servicebus.windows.net/;...", "orders-queue");

await client.SendAsync(new OrderCreated(Guid.NewGuid()));

await client.RegisterHandlerAsync(async (message, ct) =>
{
    var order = message.GetBody<OrderCreated>();
    return MessageReceivedStatus.Completed; // or Abandoned / Deadletter
}, (ex, ct) => Task.CompletedTask);
```

Equivalent standalone clients exist for topics (`ServiceBusStandaloneTopicAsyncClient`, publish-only) and subscriptions (`ServiceBusStandaloneSubscriptionAsyncClient`, consume-only).

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

Calling `orderCreated.CreateMessagebusMessage()` (used internally by `SendAsync<TModel>`) produces a `MessageBusMessage` with `target`/`routing-key`/`schema-version`/`customer-id` set as application properties and the JSON payload as the body.

## OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOpenTelemetryForServiceBusMessageBus());
```

This registers the library's `ActivitySource`. The library injects/extracts trace context into the message's application properties on both publish and consume, but it does so through `System.Diagnostics.Activity` and `OpenTelemetry.Context.Propagation.Propagators.DefaultTextMapPropagator` — a **process-wide static** that the library deliberately does not configure itself, to avoid silently overriding a propagator (or its composition/order) that the hosting application may already have set up.

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
    .WithMetrics(metrics => metrics.AddOpenTelemetryMetricsForServiceBusMessageBus());
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
    .AddServiceBusMessageBusHealthCheck(sp => (IServiceBusClient)sp.GetRequiredService<IMessageBusPublisherAsync>());
```

The check only reads `ServiceBusClient.IsClosed` off whichever client the accessor resolves - no network round trip (no peek/receive call) is made, so it stays cheap enough to poll frequently. This is a weak signal: `IsClosed` only flips when `Close`/`Dispose` was explicitly called or a fatal error already surfaced elsewhere, so a client can report healthy while transiently unable to reach the broker. The library can have several independent clients registered, so there is no single implicit "the" connection to default to - you tell it which one to check. Every concrete client (topic, queue, subscription) implements `IServiceBusClient`, so cast whichever one you registered.

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see [`license.txt`](./license.txt) for the full text.
