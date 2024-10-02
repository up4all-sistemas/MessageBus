
using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.Tests.RabbitMQ.Payloads;

namespace Up4All.Framework.MessageBus.Tests.RabbitMQ.Standalone
{
    public class ExchangeTest : TestBase
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQStandaloneTopicClient(Configuration, ExchangeType.Topic, d =>
            {
                d.Durable = true;
            });
        }

        [Test]
        public void TestSendToExchange()
        {
            var service = Provider.GetRequiredService<IMessageBusStandalonePublisher>();

            var payload = new TestPayload
            {
                MyProperty = "Teste",
                MyProperty1 = "Teste1"
            };

            service.Send(payload);
            Assert.Pass();
        }
    }


}
