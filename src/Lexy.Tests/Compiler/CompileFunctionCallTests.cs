using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileFunctionCallTests : ScopedServicesTestFixture
{
    [Test]
    public async Task LibraryFunctionPower()
    {
        using var script = await ServiceProvider.CompileFunction($@"
function SimpleFunction
  parameters
    number Value
  results
    number Result
  Result = Math.Power(Value, 5)");
        var result = script.Run(new Dictionary<string, object> { { "Value", 2 } });
        var value = (decimal)result.GetValue("Result");
        value.ShouldBe(32);
    }

    [Test]
    public async Task NestedLibraryFunctionPower()
    {
        using var script = await ServiceProvider.CompileFunction($@"
function SimpleFunction
  parameters
    string Value
  results
    number Result
  Result = Math.Power(Number.Parse(Value), 5)");
        var result = script.Run(new Dictionary<string, object> { { "Value", "2" } });
        var value = (decimal)result.GetValue("Result");
        value.ShouldBe(32);
    }

    [Test]
    public async Task LexyFunctionCallSpreadResults()
    {
      using var script = await ServiceProvider.CompileFunction($@"
function Calling
  parameters
    number Value
  results
    number Result
  Result = Value + 7 

function Caller
  parameters
    number Value
  results
    number Result
  Calling.Parameters params
  params.Value = Value
  ... = Calling(params)");

      var result = script.Run(new Dictionary<string, object> { { "Value", 2 } });
      var value = (decimal)result.GetValue("Result");
      value.ShouldBe(9);
    }

    [Test]
    public async Task LexyFunctionCallSpreadParameters()
    {
      using var script = await ServiceProvider.CompileFunction($@"
function Calling
  parameters
    number Value
  results
    number Result
  Result = Value + 7 

function Caller
  parameters
    number Value
  results
    number Result
  var result = Calling(...)
  Result = result.Result");

      var result = script.Run(new Dictionary<string, object> { { "Value", 2 } });
      var value = (decimal)result.GetValue("Result");
      value.ShouldBe(9);
    }

    [Test]
    public async Task LexyFunctionCallSpreadResultsAndParameters()
    {
      using var script = await ServiceProvider.CompileFunction($@"
function Calling
  parameters
    number Value
  results
    number Result
  Result = Value + 7 

function Caller
  parameters
    number Value
  results
    number Result
  ... = Calling(...)");

      var result = script.Run(new Dictionary<string, object> { { "Value", 2 } });
      var value = (decimal)result.GetValue("Result");
      value.ShouldBe(9);
    }
}
