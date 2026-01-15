using System.Threading.Tasks;
using Lexy.Tests.Compiler;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Specifications;

public class RunScenarioResults : ScopedServicesTestFixture
{
    [Test]
    public async Task TestSimpleReturn()
    {
        var script = await ServiceProvider.CompileFunction(@"
function TestSimpleReturn
  results
    number Result
  Result = 777");

        var result = script.Run();
        result.Number("Result").ShouldBe(777);
    }
}
