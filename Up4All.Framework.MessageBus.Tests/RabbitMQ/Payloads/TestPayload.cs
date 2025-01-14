using Up4All.Framework.MessageBus.Abstractions.Attributes;

namespace Up4All.Framework.MessageBus.Tests.RabbitMQ.Payloads
{
    [MessageBusPayloadAttribute("TestPayload")]
    [MessageBusAdditionalUserPropertyAttribute("teste1", "teste23")]
    //[MessageBusRoutingKey("teste")]
    public class TestPayload
    {
        [MessageBusUserProperty("teste2")]
        public string MyProperty { get; set; } = null!;

        public string MyProperty1 { get; set; } = null!;
    }
}
