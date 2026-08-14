# Up4All.Framework.MessageBus.Kafka

Apache Kafka provider for [Up4All.MessageBus](https://github.com/up4all-sistemas/Up4All.MessageBus) — a lightweight abstraction for publishing and consuming messages that lets application code depend only on `Up4All.Framework.MessageBus.Abstractions`, while this package wires it up to Kafka using [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka) (librdkafka).

## What it does

- **Topics with consumer groups** — publish to a topic and consume it as part of a named consumer group (the subscription name becomes the Kafka `GroupId`), reading from the earliest available offset by default.
- **Typed message keys** — the default client uses a `string` key, but generic variants let you key messages by any reference type (`KafkaGenericTopicAsyncClient<TMessageKey>` / `KafkaGenericSubscriptionAsyncClient<TMessageKey>`) or value type (`KafkaWithStructKeyTopicAsyncClient<TMessageKey>` / `KafkaWithStructKeySubscriptionAsyncClient<TMessageKey>`, e.g. `Guid`, `int`, `long`).
- **Fluent pipeline builder** registered through `IServiceCollection`, binding connection settings straight from configuration (`IOptions<KafkaMessageBusOptions>`).
- **Background consumption** via a ready-made `IHostedService` (`AddDefaultHostedService()`), or drive it yourself.
- **Standalone clients** for when you don't want DI/hosting at all — just a bootstrap servers string and a topic/subscription name. They implement both `IDisposable` and `IAsyncDisposable`.
- **Message attributes** (`[MessageBusPayload]`, `[MessageBusRoutingKey]`, `[MessageBusUserProperty]`, `[MessageBusAdditionalUserProperty]`) to map POCOs to Kafka message headers without hand-building `MessageBusMessage`.
- **Built-in OpenTelemetry tracing** for publish/consume spans, with W3C context propagated through Kafka message headers.
- **Keyed DI support** (`AddPublisher(key, ...)`, `ListenSubscription(key, ...)`) for talking to more than one cluster/topic from the same process.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.Kafka --version <latest-version>
```

This pulls in `Up4All.Framework.MessageBus.Abstractions` transitively.

## Configuration

```json
{
  "MessageBusOptions": {
    "ConnectionString": "broker1:9092,broker2:9092",
    "TopicName": "orders-topic",
    "SubscriptionName": "orders-consumer-group",
    "SchemaRegistryUrl": "http://schema-registry:8081"
  }
}
```

`ConnectionString` maps to Confluent.Kafka's `BootstrapServers`; `SubscriptionName` maps to the consumer's `GroupId`. `SchemaRegistryUrl` is currently informational (reserved for future schema-registry-aware serialization) and isn't required.

## Getting started (dependency injection)

### Publishing

```csharp
// Program.cs
builder.Services.AddKafkaMessageBus() // binds the "MessageBusOptions" section by default
    .Producers.AddPublisher();        // publishes to MessageBusOptions:TopicName, string-keyed
```

```csharp
public class OrderService(IMessageBusPublisherAsync publisher)
{
    public Task NotifyOrderCreatedAsync(OrderCreated evt, CancellationToken ct)
        => publisher.SendAsync(evt, ct);
}
```

`SendAsync<TModel>` calls `model.CreateMessagebusMessage()` under the hood, serializing the payload as JSON and applying any `[MessageBus*]` attributes declared on `TModel` (see [Message attributes](#message-attributes)). The Kafka message key comes from the message id (`SetMessageId`/`[MessageBusUserProperty]` on a suitable field) — the plain string-keyed client assigns a random `Guid` as the key automatically when none was set.

### Consuming

```csharp
builder.Services.AddKafkaMessageBus()
    .Subscriptions.ListenSubscription()  // binds to MessageBusOptions:TopicName/SubscriptionName
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

`AddDefaultHostedService()` registers a background `IHostedService` that calls `RegisterHandlerAsync` on startup. Internally the consumer loop only commits the offset when the handler returns `MessageReceivedStatus.Completed` — any other result (or an exception, caught and routed to `OnErrorAsync`) leaves the offset uncommitted so the message is re-delivered on the next poll.

### Typed message keys

```csharp
builder.Services.AddKafkaMessageBus()
    .Producers.AddPublisher<Guid>();          // publishes with a Guid key

builder.Services.AddKafkaMessageBus()
    .Subscriptions.ListenSubscriptionWithStructKey<Guid>()
    .AddHandler<OrderCreatedHandler>()
    .AddDefaultHostedService();
```

### Multiple connections (keyed services)

```csharp
pipeline.Subscriptions.ListenSubscription("orders-consumer", "broker1:9092", "orders-topic", "orders-group");
pipeline.Producers.AddPublisher((object)"notifications-publisher", "other-broker:9092", "notifications-topic");
```

```csharp
public class OrdersWorker([FromKeyedServices("orders-consumer")] IMessageBusAsyncConsumer consumer) { /* ... */ }
```

## Standalone clients (no DI required)

```csharp
await using var client = new KafkaStandaloneTopicAsyncClient("broker1:9092", "orders-topic");

await client.SendAsync(new OrderCreated(Guid.NewGuid()));
```

```csharp
await using var consumer = new KafkaStandaloneSubscriptionAsyncClient("broker1:9092", "orders-topic", "orders-group");

await consumer.RegisterHandlerAsync(async (message, ct) =>
{
    var order = message.GetBody<OrderCreated>();
    return MessageReceivedStatus.Completed; // or Abandoned to leave the offset uncommitted
}, (ex, ct) => Task.CompletedTask);
```

`RegisterHandlerAsync` runs a blocking consume loop until the supplied `CancellationToken` is cancelled — call it from a background task/hosted service rather than awaiting it inline on a request path.

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

Calling `orderCreated.CreateMessagebusMessage()` (used internally by `SendAsync<TModel>`) produces a `MessageBusMessage` with `target`/`routing-key`/`schema-version`/`customer-id` set as Kafka message headers (JSON-serialized) and the JSON payload as the value.

## OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOpenTelemetryForMessageBus());
```

This registers the library's `ActivitySource`. The library injects/extracts trace context into Kafka message headers on both publish and consume, but it does so through `System.Diagnostics.Activity` and `OpenTelemetry.Context.Propagation.Propagators.DefaultTextMapPropagator` — a **process-wide static** that the library deliberately does not configure itself, to avoid silently overriding a propagator (or its composition/order) that the hosting application may already have set up.

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
    .WithMetrics(metrics => metrics.AddOpenTelemetryMetricsForMessageBus());
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
    .AddKafkaMessageBusHealthCheck("broker1:9092,broker2:9092");
```

Unlike RabbitMQ/Service Bus, this doesn't read a flag off an already-registered client - Confluent.Kafka's producer/consumer expose no "is connected" property to check - so it makes a real (if cheap) `AdminClient.GetMetadata` call against the given bootstrap servers on every check, using a short-lived `AdminClient` built just for that call. Pass a `timeout` (defaults to 5 seconds) to bound how long a check can take when the cluster is unreachable:

```csharp
builder.Services.AddHealthChecks()
    .AddKafkaMessageBusHealthCheck("broker1:9092,broker2:9092", timeout: TimeSpan.FromSeconds(2));
```

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see [`license.txt`](./license.txt) for the full text.
