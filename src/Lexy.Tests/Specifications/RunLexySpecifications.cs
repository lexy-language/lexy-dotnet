using Lexy.Compiler.Specifications;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class RunLexySpecifications : ScopedServicesTestFixture
{
    [Test]
    public void AllSpecifications()
    {
        LoggingConfiguration.LogFileNames();

        var runner = GetService<ISpecificationsRunner>();
        runner.RunAll("../../../lexy-language/src/Specifications");
        //runner.RunAll("/Users/timcools/_/Lexy/lexy-language/src/Specifications");
    }
}