using System.Linq;

using Up4All.Framework.MessageBus.RabbitMQ.Consts;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class StreamDeclareOptions : QueueDeclareOptions
    {
        internal StreamDeclareOptions() : base()
        {
            Type = QueueType.Stream;
            Args ??= [];

            if (!Args.ContainsKey("x-stream-type"))
                Args.Add("x-stream-type", "stream");
        }

        public static implicit operator StreamDeclareOptions(ProvisioningOptions opts)
        {
            if (opts is null) return null;

            return new StreamDeclareOptions
            {
                Exclusive = opts.Exclusive,
                Durable = opts.Durable,
                AutoDelete = opts.AutoDelete,
                Args = opts.Args,
                Bindings = [.. opts.Bindings.Select(x => (QueueBindOptions)x)]
            };
        }
    }
}
