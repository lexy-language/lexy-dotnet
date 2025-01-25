using Lexy.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests;

public static class ServiceProviderConfiguration
{
    public static ServiceProvider CreateServices()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(LoggingConfiguration.CreateLoggerFactory())
            .AddLexy()
            .BuildServiceProvider();

        return serviceProvider;
    }
}