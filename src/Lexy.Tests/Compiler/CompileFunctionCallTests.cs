using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileFunctionCallTests : ScopedServicesTestFixture
{
    [Test]
    public void LibraryFunctionPower()
    {
        using var script = ServiceProvider.CompileFunction($@"
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
    public void NestedLibraryFunctionPower()
    {
        using var script = ServiceProvider.CompileFunction($@"
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
    public void LexyFunctionCallSpreadResults()
    {
      using var script = ServiceProvider.CompileFunction($@"
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
    public void LexyFunctionCallSpreadParameters()
    {
      using var script = ServiceProvider.CompileFunction($@"
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
    public void LexyFunctionCallSpreadResultsAndParameters()
    {
      using var script = ServiceProvider.CompileFunction($@"
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