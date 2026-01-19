using System;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneClientBase : IDisposable
    {
        protected MessageBusStandaloneClientBase()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        ~MessageBusStandaloneClientBase()
        {
            Dispose(false);
        }
    }
}
