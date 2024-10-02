using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.Tests.RabbitMQ.Payloads;

namespace Up4All.Framework.MessageBus.Tests.RabbitMQ.Standalone
{
    public class QueueTest : TestBase
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQStandaloneQueueClient(Configuration, d => 
            {
                d.AddBinding("testtopic", b => { b.RoutingKey = "teste"; });
            });
        }

        [Test]
        public void TestSendToQueue()
        {
            var service = Provider.GetRequiredService<IMessageBusStandaloneQueueClient>();

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
