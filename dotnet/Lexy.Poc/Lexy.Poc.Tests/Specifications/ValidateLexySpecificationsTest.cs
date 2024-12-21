using Lexy.Poc.Core.Specifications;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Lexy.Poc.Specifications
{
    public class ValidateLexySpecificationsTest : ScopedServicesTestFixture
    {
        [Test]
        public void AllSpecifications()
        {
            LoggingConfiguration.LogFileNames();

            var runner = ServiceProvider.GetRequiredService<ISpecificationsRunner>();
            runner.RunAll("../../../../../../laws/Specifications");
        }

        [Test]
        public void AllSpecificFile()
        {
            LoggingConfiguration.LogFileNames();

            var runner = ServiceProvider.GetRequiredService<ISpecificationsRunner>();
            runner.Run("../../../../../../laws/Specifications/Language/00004-BooleanLiteral.lexy");
        }
    }
}