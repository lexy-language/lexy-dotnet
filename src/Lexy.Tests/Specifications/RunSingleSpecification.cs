using Lexy.Compiler.Specifications;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class RunSingleSpecification : ScopedServicesTestFixture
{
    [Test]
    //[Ignore("Used for debugging a specific file from IDE")]
    public void SpecificFile()
    {
        LoggingConfiguration.LogFileNames();

        var runner = GetService<ISpecificationsRunner>();
        runner.Run("../../../lexy-language/Specifications/Table/LookupValidation.lexy");
        //runner.Run("../../../lexy-language/Specifications/Isolated.lexy");

        //runner.Run("/Users/timcools/_/Lexy/lexy-language/src/Specifications/Table/Syntax.lexy");
        //runner.Run("../../../lexy-language/src/Specifications/Isolate.lexy");
        //runner.Run("../../../lexy-language/src/Specifications/Function/Variables.lexy");
        //runner.Run("../../../lexy-language/src/Specifications/BuiltInFunctions/Extract.lexy");
    }
}