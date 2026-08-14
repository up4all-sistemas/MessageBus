using System;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneClientBase : IDisposable, IAsyncDisposable
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

        public async ValueTask DisposeAsync()
        {
            // Unlike the textbook IAsyncDisposable pattern, Dispose(bool) here doesn't split
            // managed/unmanaged cleanup - it always performs the same (synchronous) close.
            // Calling it after DisposeAsyncCore() would therefore close the connection twice,
            // synchronously, defeating the purpose of the async path. DisposeAsyncCore() alone
            // is responsible for all cleanup here.
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Providers that have not yet been migrated to a real async close (see RabbitMQ's
        /// standalone clients for the reference implementation) fall back to the existing
        /// synchronous <see cref="Dispose(bool)"/> - same blocking behavior as before, just
        /// reachable from <see cref="DisposeAsync"/> too, so no provider regresses or leaks
        /// a connection while it hasn't been migrated yet.
        /// </summary>
        protected virtual ValueTask DisposeAsyncCore()
        {
            Dispose(true);
            return default;
        }

        ~MessageBusStandaloneClientBase()
        {
            Dispose(false);
        }
    }
}
