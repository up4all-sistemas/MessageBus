using System.Collections.Generic;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class StreamDeclareOptions : QueueDeclareOptions
    {
        public StreamDeclareOptions() : base()
        {
            if (Args == null)
                Args = new Dictionary<string, object>();

            if (!Args.ContainsKey("x-stream-type"))
                Args.Add("x-stream-type", "stream");
        }
    }
}
