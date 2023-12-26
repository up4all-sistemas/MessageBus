using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Factories;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Xunit;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests
{
    public class RabbitMQNamedInstanceTest
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public RabbitMQNamedInstanceTest()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.local.json", false, true)
            .Build();

            var services = new ServiceCollection();

            services.AddLogging();

            services.AddMessageBusNamedQueueClient(_configuration, "queue1", (provider, opts) =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneQueueClient>>();

                return new RabbitMQStandaloneQueueClient(opts.ConnectionString
                    ,opts.QueueName, 8);
            });

            services.AddMessageBusNamedTopicClient(_configuration, "topic1", (provider, opts) =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneTopicClient>>();

                return new RabbitMQStandaloneTopicClient(
                      opts.ConnectionString
                    , opts.TopicName
                    , 8);
            });

            services.AddMessageBusNamedSubscriptionClient(_configuration, "sub1", (provider, opts) =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneSubscriptionClient>>();

                return new RabbitMQStandaloneSubscriptionClient(
                      opts.ConnectionString
                    , opts.SubscriptionName
                    , 8);
            });

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async void QueueSendMessage()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetQueueClient("queue1");

            var msg = new MessageBusMessage()
            {
            };
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public void QueueReceiveMessage()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetQueueClient("queue1");

            client.RegisterHandler((msg) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex) => Debug.Print(ex.Message), () => { }, false);


            Thread.Sleep(5000);
        }

        [Fact]
        public void QueueReceiveMessageWithError()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetQueueClient("queue1");

            client.RegisterHandler((msg) =>
            {
                throw new Exception("Test Error");
            }, (ex) => Debug.Print(ex.Message), () => { }, false);


            Thread.Sleep(5000);
        }

        [Fact]
        public void QueueReceiveMessageWithDeadLetter()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetQueueClient("queue1");

            client.RegisterHandler((msg) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Deadletter;
            }, (ex) => Debug.Print(ex.Message), () => { }, false);


            Thread.Sleep(5000);
        }

        [Fact]
        public async void TopicSendMessage()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetTopicClient("topic1");

            var msg = new MessageBusMessage()
            {
            };
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");
            //msg.UserProperties.Add("routing-key", "test-subs");
            msg.AddUserProperty("target", "main");

            await client.SendAsync(msg);

            Assert.True(true);
        }

        [Fact]
        public void SubscriptioneReceiveMessage()
        {
            var factory = _provider.GetRequiredService<MessageBusFactory>();
            var client = factory.GetSubscriptionClient("sub1");

            client.RegisterHandler((msg) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex) => Debug.Print(ex.Message));


            Thread.Sleep(5000);
        }
    }
}
