using Lexy.Compiler.Specifications;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class RunLexyIntroductionScenarios : ScopedServicesTestFixture
{
    [Test]
    public void RunAll()
    {
        LoggingConfiguration.LogFileNames();

        var runner = GetService<ISpecificationsRunner>();
        runner.RunAll("../../../lexy-language/Introduction");
    }
}