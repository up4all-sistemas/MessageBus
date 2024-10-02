using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Up4All.Framework.MessageBus.Tests
{
    public abstract class TestBase
    {
        protected readonly IServiceProvider Provider;
        protected readonly IConfiguration Configuration;

        protected TestBase()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.local.json", false)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton(Configuration);
            ConfigureServices(services);

            Provider = services.BuildServiceProvider();
        }

        protected abstract void ConfigureServices(IServiceCollection services);

    }
}