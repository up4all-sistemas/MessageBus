using System;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions.Factories
{
    internal class NamedInstanceClient<TClient> where TClient : class
    {
        public string Key { get; set; }

        public TClient Instance { get; set; }
    }
}
