using Lexy.Compiler.Specifications;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Lexy.Poc.Specifications
{
    public class RunLexySpecifications : ScopedServicesTestFixture
    {
        [Test]
        public void AllSpecifications()
        {
            LoggingConfiguration.LogFileNames();

            var runner = GetService<ISpecificationsRunner>();
            runner.RunAll("../../../../../../laws/Specifications");
        }

        [Test]
        public void SpecificFile() // used for debugging a specific file from IDE
        {
            LoggingConfiguration.LogFileNames();

            var runner = GetService<ISpecificationsRunner>();
            //runner.Run("../../../../../../laws/Specifications/Table/LookupValidation.lexy");
            runner.Run("../../../../../../laws/Specifications/Function/FunctionCall.lexy");
            //runner.Run("../../../../../../laws/Specifications/Function/Variables.lexy");
        }
    }
}