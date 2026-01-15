using System.Threading.Tasks;
using Lexy.Compiler.Specifications;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class RunSingleSpecification : ScopedServicesTestFixture
{
    [Test]
    //Ignore("Used for debugging a specific file from IDE")]
    public async Task SpecificFile()
    {
        LoggingConfiguration.LogFileNames();

        var runner = GetService<ISpecificationsRunner>();
        await runner.Run("../../../lexy-language/Specifications/Function/FunctionCallSpread.lexy");
        //await runner.Run("../../../lexy-language/Specifications/Isolated.lexy");

        //await runner.Run("/Users/timcools/_/Lexy/lexy-language/src/Specifications/Table/Syntax.lexy");
        //await runner.Run("../../../lexy-language/src/Specifications/Isolate.lexy");
        //await runner.Run("../../../lexy-language/src/Specifications/Function/Variables.lexy");
        //await runner.Run("../../../lexy-language/src/Specifications/BuiltInFunctions/Extract.lexy");
    }
}
