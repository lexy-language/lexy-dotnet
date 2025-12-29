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
}