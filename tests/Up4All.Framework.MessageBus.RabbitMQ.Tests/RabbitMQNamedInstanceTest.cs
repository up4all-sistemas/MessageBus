using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Factories;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
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

            services.AddMessageBusNamedQueueClient("queue1", provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneQueueClient>>();

                return new RabbitMQStandaloneQueueClient(_configuration.GetValue<string>("MessageBusOptions:ConnectionString")
                    ,_configuration.GetValue<string>("MessageBusOptions:QueueName"), 8);
            });

            services.AddMessageBusNamedTopicClient("topic1", provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneTopicClient>>();

                return new RabbitMQStandaloneTopicClient(
                      _configuration.GetValue<string>("MessageBusOptions:ConnectionString")                     
                    , _configuration.GetValue<string>("MessageBusOptions:TopicName")
                    , 8);
            });

            services.AddMessageBusNamedSubscriptionClient("sub1", provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQStandaloneSubscriptionClient>>();

                return new RabbitMQStandaloneSubscriptionClient(
                      _configuration.GetValue<string>("MessageBusOptions:ConnectionString")
                    , _configuration.GetValue<string>("MessageBusOptions:SubscriptionName")
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
