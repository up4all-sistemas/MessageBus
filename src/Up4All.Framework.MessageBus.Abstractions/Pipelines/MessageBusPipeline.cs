using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Collections.Generic;

using Up4All.Framework.MessageBus.Abstractions.Interfaces.Pipelines;

namespace Up4All.Framework.MessageBus.Abstractions.Pipelines
{
    public abstract class MessageBusPipeline<TOptions>(IServiceCollection services, string configurationBindKey)
        where TOptions : class
    {
        protected readonly List<IPipelineBuilder> _pipelines = [];

        public string ConfigurationBindKey { get; private set; } = configurationBindKey;

        public IServiceCollection Services { get; private set; } = services;

        public OptionsBuilder<TOptions> OptionsBuilder { get; private set; }

        protected MessageBusPipeline<TOptions> AddOptions()

        {
            OptionsBuilder = Services.AddOptions<TOptions>()
                .BindConfiguration(ConfigurationBindKey)
                .ValidateDataAnnotations();

            return this;
        }

        public virtual void Validate()
        {
            _pipelines.ForEach(p => p.Validate());
        }
    }
}
