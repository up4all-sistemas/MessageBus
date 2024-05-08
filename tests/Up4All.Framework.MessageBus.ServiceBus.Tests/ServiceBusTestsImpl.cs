using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Xunit;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests
{
    public class ServiceBusTestsImpl
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public ServiceBusTestsImpl()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();



            var services = new ServiceCollection();

            services.AddMessageBusQueueClient<ServiceBusQueueClient>(_configuration);
            services.AddMessageBusTopicClient<ServiceBusTopicClient>(_configuration);
            services.AddMessageBusSubscribeClient<ServiceBusSubscribeClient>(_configuration);

            services.AddStandaloneQueueClient((provider) => new ServiceBusStandaloneQueueClient(_configuration.GetValue<string>("MessageBusOptions:ConnectionString"), _configuration.GetValue<string>("MessageBusOptions:QueueName")
                , _configuration.GetValue<int>("MessageBusOptions:ConnectionAttempts")));

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task QueueSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            var msg = new MessageBusMessage()
            {
            };
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public async Task QueueSendMessageTyped()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            var model = new TestModel();
            await client.SendAsync(model);

            Assert.True(true);
        }

        [Fact]
        public async Task QueueSendMessageList()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            var list = Enumerable.Range(0, 3)
                .Select(x => new TestModel());

            await client.SendManyAsync(list);

            Assert.True(true);
        }

        [Fact]
        public void QueueReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            client.RegisterHandler((msg) =>
            {
                Assert.NotNull(msg);
                return Abstractions.Enums.MessageReceivedStatus.Completed;
            }, (ex) => Debug.Print(ex.Message));
        }

        [Fact]
        public void QueueReceiveMessageTyped()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            client.RegisterHandler<TestModel>((msg) =>
            {
                Assert.True(msg is not null);
                return Abstractions.Enums.MessageReceivedStatus.Completed;
            }, (ex) => Debug.Print(ex.Message));
        }

        [Fact]
        public async Task TopicSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusPublisher>();

            var msg = new MessageBusMessage();
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("action", "dev-sub-test");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public void SubscriptionReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusConsumer>();

            client.RegisterHandler((msg) =>
            {
                Assert.NotNull(msg);
                return Abstractions.Enums.MessageReceivedStatus.Completed;
            }, (ex) => Debug.Print(ex.Message));
        }

        [Fact]
        public async Task StandaloneQueueSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusStandaloneQueueClient>();

            var msg = new MessageBusMessage()
            {
            };
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");

            await client.SendAsync(msg);

            Assert.True(true);
        }
    }

    public class TestModel
    {
        public int Prop1 { get; set; }

        public string Prop2 { get; set; }

        public TestModel()
        {
            Prop1 = 1;
            Prop2 = Guid.NewGuid().ToString();
        }
    }
}
