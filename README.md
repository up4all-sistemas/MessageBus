# Up4All.MessageBus

Up4All.MessageBus is a .NET library designed to facilitate message-based communication between components or services. It supports multiple .NET targets, including .NET Standard 2.0, .NET Standard 2.1, and .NET 8.

## Features

- Publish/Subscribe messaging pattern
- Decoupled communication between modules
- Extensible and easy to integrate
- Support for multiple .NET platforms

## Getting Started

### Installation

To install Up4All.MessageBus with RabbitMQ or Azure Service Bus support, use the following NuGet commands:

#### Abstractions

```bash
dotnet add package Up4All.MessageBus.Abstractions --version <latest-version>
```

#### RabbitMQ

```bash
dotnet add package Up4All.MessageBus.RabbitMQ --version <latest-version>
```

#### Azure Service Bus

```bash
dotnet add package Up4All.MessageBus.ServiceBus --version <latest-version>
```

Replace `<latest-version>` with the desired version number.

### Get Started

Add the following code to your Dependency Injection:

````json
{
  "MessageBusOptions": {
	"ConnectionString": "<Your_Connection_String>",
	"QueueName": "<Your Azure ServiceBus Queue Name or RabbitMQ Queue Name>"
  }
}
````

````csharp
    //For ServiceBus Queue Consumers
	builder.Services.AddMessageBusQueueClient<ServiceBusQueueClient>(builder.Configuration);

	//For RabbitMQ Queue Consumers
	builder.Services.AddMessageBusQueueClient<RabbitMQQueueClient>(builder.Configuration);


	In your consumer class, just inject the IMessageBusQueueConsumer and register your handler like below:
	public class YourConsumerClass
	{
		private readonly IMessageBusQueueConsumer _messageBusQueueConsumer;

		public YourConsumerClass(IMessageBusQueueConsumer messageBusQueueConsumer)
		{
			_messageBusQueueConsumer = messageBusQueueConsumer;
		}

		public void StartConsuming()
		{
			_messageBusQueueConsumer.RegisterHandler((ReceivedMessage message) => {
				return MessageReceivedStatus.Completed; //or MessageReceivedStatus.Abandoned based on your logic
			}, (Exception ex) => { //do something with exception }, () => { //Do something in idle }, autoComplete: false);
			
		}
	}
````

## Target Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 8

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements or bug fixes.

## License

This project is licensed under the MIT License.