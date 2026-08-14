# Up4All.MessageBus

Up4All.MessageBus is a .NET messaging abstraction library. Application code depends only on
`Up4All.Framework.MessageBus.Abstractions` — publishers, consumers, message envelopes, DI
pipelines — and one of the provider packages wires that abstraction up to an actual broker.
Swapping RabbitMQ for Azure Service Bus or Kafka (or running more than one at once) doesn't
touch application/business code.

## Packages

| Package | What it wires up | Docs |
|---|---|---|
| [`Up4All.Framework.MessageBus.Abstractions`](src/Up4All.Framework.MessageBus.Abstractions) | Core abstractions: `MessageBusMessage`/`ReceivedMessage`, publisher/consumer interfaces, DI pipeline builder, OpenTelemetry helpers. Pulled in transitively by every provider. | [README](src/Up4All.Framework.MessageBus.Abstractions/README.md) |
| [`Up4All.Framework.MessageBus.RabbitMQ`](src/Up4All.Framework.MessageBus.RabbitMQ) | RabbitMQ — queues, exchanges/topics and streams, via [RabbitMQ.Client](https://www.nuget.org/packages/RabbitMQ.Client). | [README](src/Up4All.Framework.MessageBus.RabbitMQ/README.md) |
| [`Up4All.Framework.MessageBus.ServiceBus`](src/Up4All.Framework.MessageBus.ServiceBus) | Azure Service Bus — queues and topics/subscriptions, via [Azure.Messaging.ServiceBus](https://www.nuget.org/packages/Azure.Messaging.ServiceBus). | [README](src/Up4All.Framework.MessageBus.ServiceBus/README.md) |
| [`Up4All.Framework.MessageBus.Kafka`](src/Up4All.Framework.MessageBus.Kafka) | Apache Kafka — topics with consumer groups, typed message keys, via [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka). | [README](src/Up4All.Framework.MessageBus.Kafka/README.md) |
| [`Up4All.Framework.MessageBus.TransferHelper`](src/Up4All.Framework.MessageBus.TransferHelper) | Forwards ("shovels") messages from one broker/entity to another — e.g. RabbitMQ → Service Bus — with an optional transformation step in between. | [README](src/Up4All.Framework.MessageBus.TransferHelper/README.md) |

## Features

- **Publish/Subscribe** across queues, topics/exchanges, subscriptions and (RabbitMQ) streams.
- **One abstraction, several brokers** — application code depends on `Up4All.Framework.MessageBus.Abstractions` only.
- **Fluent DI pipeline builder** (`AddRabbitMQMessageBus()`, `AddServiceBusMessageBus()`, `AddKafkaMessageBus()`) bound straight from configuration.
- **Background consumption** via a ready-made `IHostedService`, or drive the consumer loop yourself.
- **Standalone clients** (`RabbitMQStandaloneQueueAsyncClient`, etc.) for when you don't want DI/hosting at all — just a connection string and an entity name. Every client implements `IAsyncDisposable`.
- **Message attributes** (`[MessageBusPayload]`, `[MessageBusRoutingKey]`, `[MessageBusUserProperty]`, `[MessageBusAdditionalUserProperty]`) to map POCOs to broker metadata without hand-building a `MessageBusMessage`.
- **OpenTelemetry tracing and metrics** built in for every provider, following the OpenTelemetry messaging semantic conventions.
- **Health checks** per provider, reporting broker connectivity through `Microsoft.Extensions.Diagnostics.HealthChecks` (no ASP.NET Core dependency — works from a plain console/worker host too).
- **Keyed DI support** for talking to more than one broker/entity from the same process.
- **Cross-broker transfer** (`Up4All.Framework.MessageBus.TransferHelper`) for shoveling/forwarding messages between two message bus instances.

## Installation

```bash
dotnet add package Up4All.Framework.MessageBus.Abstractions --version <latest-version>

# and one (or more) of:
dotnet add package Up4All.Framework.MessageBus.RabbitMQ --version <latest-version>
dotnet add package Up4All.Framework.MessageBus.ServiceBus --version <latest-version>
dotnet add package Up4All.Framework.MessageBus.Kafka --version <latest-version>
```

Each provider package pulls in `Up4All.Framework.MessageBus.Abstractions` transitively, so
referencing it explicitly is only needed if your project only touches the abstractions (e.g. a
shared contracts project).

## Quick start

Configuration (`appsettings.json`) is the same shape across providers — only the connection
string format and entity names change:

```json
{
  "MessageBusOptions": {
    "ConnectionString": "amqp://guest:guest@localhost:5672",
    "QueueName": "orders-queue",
    "TopicName": "orders-exchange"
  }
}
```

### Publishing

```csharp
// Program.cs
builder.Services.AddRabbitMQMessageBus()   // or AddServiceBusMessageBus() / AddKafkaMessageBus()
    .Producers.AddPublisher();             // publishes to MessageBusOptions:TopicName
```

```csharp
public class OrderService(IMessageBusPublisherAsync publisher)
{
    public Task NotifyOrderCreatedAsync(OrderCreated evt, CancellationToken ct)
        => publisher.SendAsync(evt, ct);
}
```

### Consuming

```csharp
builder.Services.AddRabbitMQMessageBus()   // or AddServiceBusMessageBus() / AddKafkaMessageBus()
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

`AddDefaultHostedService()` registers a background `IHostedService` that drives the consumer
loop; a message that throws inside `OnMessageReceivedAsync` is routed to `OnErrorAsync` and the
message is abandoned/left uncommitted (broker-specific) — you don't ack/nack manually.

### Standalone clients (no DI required)

```csharp
await using var client = new RabbitMQStandaloneQueueAsyncClient(logger, "amqp://guest:guest@localhost:5672", "orders-queue", persistent: true);

await client.SendAsync(new OrderCreated(Guid.NewGuid()));

await client.RegisterHandlerAsync(async (message, ct) =>
{
    var order = message.GetBody<OrderCreated>();
    return MessageReceivedStatus.Completed; // or Abandoned / Deadletter
}, (ex, ct) => Task.CompletedTask);
```

### Message attributes

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

`orderCreated.CreateMessagebusMessage()` (called internally by `SendAsync<TModel>`) produces a
`MessageBusMessage` with the JSON payload as the body and the attributed metadata mapped onto
broker-specific headers/properties.

See each provider's README for broker-specific details (streams, topic subscriptions, typed
Kafka message keys, provisioning options, keyed multi-connection setups): [RabbitMQ](src/Up4All.Framework.MessageBus.RabbitMQ/README.md) · [ServiceBus](src/Up4All.Framework.MessageBus.ServiceBus/README.md) · [Kafka](src/Up4All.Framework.MessageBus.Kafka/README.md).

## Observability

Every provider registers OpenTelemetry tracing and metrics, plus a health check, through the
same DI pattern:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOpenTelemetryForRabbitMQMessageBus())
    .WithMetrics(metrics => metrics.AddOpenTelemetryMetricsForRabbitMQMessageBus());

builder.Services.AddHealthChecks()
    .AddRabbitMQMessageBusHealthCheck(sp => (IRabbitMQClient)sp.GetRequiredService<IMessageBusPublisherAsync>());
```

- **Tracing**: publish/consume spans following the OpenTelemetry messaging semantic
  conventions, with W3C context propagated through broker-native headers/properties. The host
  application must configure `Propagators.DefaultTextMapPropagator` itself (see the per-provider
  READMEs) — the library never mutates that process-wide static on its own.
- **Metrics**: `messaging.client.sent.messages`, `messaging.client.consumed.messages` and
  `messaging.client.operation.duration`, tagged with `messaging.system`,
  `messaging.destination.name`, `messaging.operation.type` and (on failure) `error.type`.
- **Health checks**: reports broker connectivity (RabbitMQ: connection `IsOpen`; Service Bus:
  client `IsClosed`; Kafka: `AdminClient.GetMetadata`) through
  `Microsoft.Extensions.Diagnostics.HealthChecks` — no ASP.NET Core dependency, so it works from
  a plain console/worker host too.

## Cross-broker transfer

`Up4All.Framework.MessageBus.TransferHelper` forwards messages from one broker/entity to
another, running as a background `IHostedService`:

```csharp
builder.Services.AddTransfer<RabbitMQMessageBusOptions, MessageBusOptions>()
    .AddSource(() => builder.Services.AddRabbitMQMessageBus("SourceOptions").Queues.ListenQueue())
    .AddDestination(() => builder.Services.AddServiceBusMessageBus("DestinationOptions").Producers.AddPublisher());
```

Implement `ITransformationHandler`/`IBeforeTransferHandler` to reshape or inspect a message
before it's forwarded; both have sane defaults if you don't need to customize them.

## Target Frameworks

- Library projects (`Abstractions`, `RabbitMQ`, `ServiceBus`, `Kafka`, `TransferHelper`): **.NET Standard 2.1**
- Test suite: **.NET 8**

## Building & Testing

```bash
dotnet restore Up4All.MessageBus.sln
dotnet build Up4All.MessageBus.sln -c Release
dotnet test Up4All.MessageBus.sln --collect:"XPlat Code Coverage"
```

Test projects live under [`tests/`](tests) (unit tests, mock-based — no broker required to run
the suite) alongside `src/` for the library projects.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements or bug
fixes. See [`CLAUDE.md`](CLAUDE.md) for build/test commands and coding conventions used across
this repository.

## CI/CD

[`pipelines/azure-pipelines.yaml`](pipelines/azure-pipelines.yaml) builds, tests and packs every
push/PR to `master`/`release/*`, and publishes to NuGet.org when a commit is tagged `v*`.

## License

Each package is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see the
`license.txt` included in each project (e.g.
[`src/Up4All.Framework.MessageBus.RabbitMQ/license.txt`](src/Up4All.Framework.MessageBus.RabbitMQ/license.txt))
for the full text.
