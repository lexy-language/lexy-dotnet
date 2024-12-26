using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Lexy.Poc
{
    [SetUpFixture]
    public class TestServiceProvider
    {
        private static ServiceProvider instance;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            LoggingConfiguration.RemoveOldFiles();
            LoggingConfiguration.ConfigureSerilog();

            instance = ServiceProviderConfiguration.CreateServices();
        }

        public static IServiceScope CreateScope()
        {
            return instance.CreateScope();
        }
    }
}