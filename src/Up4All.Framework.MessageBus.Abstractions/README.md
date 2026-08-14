# Up4All.Framework.MessageBus.Abstractions

Core abstraction layer for [Up4All.MessageBus](https://github.com/up4all-sistemas/Up4All.MessageBus) — the contracts, messages, pipeline base types and cross-cutting helpers that every broker provider (RabbitMQ, Azure Service Bus, Kafka) implements. Application code that depends only on this package can publish/consume messages, register handlers and wire up observability without referencing any specific broker.

## What it contains

- **Publisher/consumer contracts** (`IMessageBusPublisherAsync`, `IMessageBusAsyncConsumer`, `IMessageBusQueueAsyncClient`, `IMessageBusStreamAsyncClient`, and their standalone counterparts) — the surface every provider implements, so application code can depend on the interface instead of a concrete broker client.
- **`MessageBusMessage` / `ReceivedMessage`** — the broker-agnostic envelope (byte body + user properties/headers), with helpers to attach a JSON body, set/read a message id, read typed user properties, and stamp trace properties (`AddTraceProperties`).
- **`IMessageBusMessageHandler`** — the contract application code implements to react to a received message (`OnMessageReceivedAsync`) and to consumer errors (`OnErrorAsync`); a generic `IMessageBusMessageHandler<TMessage>` variant exists for handlers built around one already-deserialized model.
- **`DefaultConsumer`** — a ready-made `IMessageDefaultConsumer` that bridges an `IMessageBusAsyncConsumer` to an `IMessageBusMessageHandler`, abandoning the message when the handler throws. Providers surface this behind `AddDefaultHostedService()`.
- **`MessageBusPipeline<TOptions>` / `MessageBusPublisherPipeline` / `MessageBusConsumerPipeline`** — the base classes each provider's fluent `IServiceCollection` builder (`AddPublisher()`, `ListenQueue()`, ...) derives from, including options binding/validation (`IOptions<TOptions>` via `BindConfiguration` + data annotations) and handler registration.
- **`MessageBusOptions`** — the common option set (`ConnectionString`, `QueueName`, `TopicName`, `SubscriptionName`, `StreamName`, `ConnectionAttempts`) every provider's options type extends.
- **Message attributes** (`[MessageBusPayload]`, `[MessageBusRoutingKey]`, `[MessageBusUserProperty]`, `[MessageBusAdditionalUserProperty]`) and the `CreateMessagebusMessage<TModel>()` extension that reads them to build a `MessageBusMessage` from a POCO without hand-rolling one.
- **OpenTelemetry helpers** (`OpenTelemetryExtensions`, `OpenTelemetryMetricsExtensions`) — shared `ActivitySource`/`Meter` creation, W3C context injection/extraction through `MessageBusMessage` user properties, and span/metric tagging that follow the OpenTelemetry messaging semantic conventions. Providers call into these to keep their tracing/metrics shape consistent.
- **`MessageBusStandaloneClientBase`** — the `IDisposable`/`IAsyncDisposable` base for standalone clients (no DI/hosting), so `await using` closes the underlying connection/channel asynchronously instead of blocking a thread.
- **`MessageReceivedStatus`** enum (`Completed`, `Abandoned`, `Deadletter`) returned by consumer handlers to tell the provider how to settle a message.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.Abstractions --version <latest-version>
```

You normally don't reference this package directly — it comes in transitively with a broker provider (`Up4All.Framework.MessageBus.RabbitMQ`, `.ServiceBus` or `.Kafka`). Reference it on its own when you want to write broker-agnostic code (e.g. a shared handler library) that shouldn't depend on any specific provider.

## Publishing

```csharp
public class OrderService(IMessageBusPublisherAsync publisher)
{
    public Task NotifyOrderCreatedAsync(OrderCreated evt, CancellationToken ct)
        => publisher.SendAsync(evt, ct);
}
```

`SendAsync<TModel>` calls `model.CreateMessagebusMessage()` under the hood, serializing the payload as JSON and applying any `[MessageBus*]` attributes declared on `TModel` (see [Message attributes](#message-attributes)). `IMessageBusPublisherAsync` also exposes `SendAsync(MessageBusMessage)`, `SendAsync(IEnumerable<MessageBusMessage>)` and `SendManyAsync<TModel>` for when you build the envelope yourself or need to publish a batch.

## Consuming

Implement `IMessageBusMessageHandler` and let a provider's `AddDefaultHostedService()` drive it through `DefaultConsumer`:

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

A message that throws inside `OnMessageReceivedAsync` is caught by `DefaultConsumer`, routed to `OnErrorAsync`, and the message is settled as `MessageReceivedStatus.Abandoned` — you don't ack/nack manually. Drive `IMessageBusAsyncConsumer.RegisterHandlerAsync` yourself instead of `DefaultConsumer` when you need finer control over settlement (e.g. returning `Deadletter`) or want to consume already-deserialized models via the generic overload.

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

Calling `orderCreated.CreateMessagebusMessage()` (used internally by `SendAsync<TModel>`) produces a `MessageBusMessage` with `target`/`routing-key`/`schema-version`/`customer-id` set as user properties and the JSON payload as the body. Each provider maps `UserProperties` onto its own broker's native metadata (AMQP headers, application properties, Kafka headers, ...).

## Reading a received message

```csharp
var order = message.GetBody<OrderCreated>();
var correlationId = message.GetCorrelationId();
var custom = message.GetUserPropertyValueAsString("customer-id");
```

`MessageBusExtensions` also exposes typed `TryGetUserPropertyAs...` helpers (`Int32`, `Int64`, `Decimal`, `DateTime`, `Object`, or a generic `TryGetUserPropertyAs<T>` that deserializes JSON) for reading properties without assuming their raw CLR type.

## Building a broker provider

A new provider typically:

1. Derives its options from `MessageBusOptions`.
2. Derives a pipeline builder from `MessageBusPipeline<TOptions>`, and publisher/consumer sub-builders from `MessageBusPublisherPipeline`/`MessageBusConsumerPipeline`, exposing them through an `IoCExtensions.AddXxxMessageBus()` entry point.
3. Implements `IMessageBusPublisherAsync`/`IMessageBusAsyncConsumer` (or the queue/stream/standalone specializations) against the broker's SDK, mapping `MessageBusMessage.UserProperties` to and from the broker's native metadata.
4. Uses `OpenTelemetryExtensions`/`OpenTelemetryMetricsExtensions` to create its own `ActivitySource`/`Meter` and record spans/instruments with the shared tag shape.
5. Wraps `DefaultConsumer` in an `IHostedService` for `AddDefaultHostedService()`, and derives standalone clients from `MessageBusStandaloneClientBase`.

See the `Up4All.Framework.MessageBus.RabbitMQ`, `.ServiceBus` and `.Kafka` packages for reference implementations.

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see [`license.txt`](./license.txt) for the full text.
