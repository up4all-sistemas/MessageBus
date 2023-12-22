using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Configurations;

using Xunit;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests
{
    public class RabbitMQTestsImpl
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public RabbitMQTestsImpl()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.local.json", false, true)
            .Build();

            var services = new ServiceCollection();

            services.AddLogging();

            services.AddMessageBusQueueClient(_configuration);
            services.AddMessageBusTopicClient(_configuration);
            services.AddMessageBusSubscriptionClient(_configuration);
            services.AddMessageBusStreamClient(_configuration, "next");

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task QueueSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            var msg = new MessageBusMessage();
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.AddUserProperty("proptst", "tst");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public async Task StreamSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusStreamClient>();

            var msg = new MessageBusMessage();
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.AddUserProperty("proptst", "tst");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public void QueueReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            bool passed = false;

            client.RegisterHandler((msg) =>
            {
                passed = msg != null
                && msg.UserProperties.ContainsKey("proptst")
                && msg.UserProperties["proptst"] == "tst";
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex) => Debug.Print(ex.Message));

            Thread.Sleep(3000);

            Assert.True(passed);
        }

        [Fact]
        public async Task TopicSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusPublisher>();

            var msg = new MessageBusMessage();
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");
            msg.AddUserProperty("target", "subscription");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public async Task SubscriveReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusConsumer>();

            await client.RegisterHandlerAsync(async (msg, c) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex, c) => { Debug.Print(ex.Message); return Task.CompletedTask; });

            Thread.Sleep(3000);
        }

        [Fact]
        public async Task StreamReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusStreamClient>();

            MessageBusMessage result = null;

            await client.RegisterHandlerAsync(async (msg, c) =>
            {
                result = msg;
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex, c) => { Debug.Print(ex.Message); return Task.CompletedTask; });

            //await StreamSendMessage();

            while (result == null) continue;

            Assert.NotNull(result);
        }
    }
}
