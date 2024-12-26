using Lexy.Poc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc
{
    public static class ServiceProviderConfiguration
    {
        public static ServiceProvider CreateServices()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ILoggerFactory>(LoggingConfiguration.CreateLoggerFactory())
                .AddLexy()
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}