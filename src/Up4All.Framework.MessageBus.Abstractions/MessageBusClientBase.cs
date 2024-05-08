using Microsoft.Extensions.Options;

using System;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusClientBase : IDisposable
    {
        protected readonly MessageBusOptions MessageBusOptions;

        protected MessageBusClientBase(IOptions<MessageBusOptions> messageBusOptions)
        {
            MessageBusOptions = messageBusOptions.Value;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

    }
}
