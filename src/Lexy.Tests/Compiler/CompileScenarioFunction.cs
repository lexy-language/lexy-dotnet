using System.Dynamic;
using System.Threading.Tasks;
using Lexy.Compiler.Generation;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileScenarioFunction : ScopedServicesTestFixture
{
    [Test]
    public async Task TestSimpleReturn()
    {
      var script = await ServiceProvider.CompileFunction(@"scenario NewScenario
  function
    parameters
      number Input
    results
      number Output
    Output = Input + 100
  parameters
    Input = 10
  results
    Output = 110");

      dynamic parameters = new ExpandoObject();
      parameters.Input = 10;

      FunctionResult result = script.Run(parameters);
      result.Number("Output").ShouldBe(110);
    }
}
