using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public async Task TestSimpleReturn()
    {
        using var script = await ServiceProvider.CompileFunction(@"function TestSimpleReturn
  results
    number Result
  Result = 777");
        var result = script.Run();
        result.Number("Result").ShouldBe(777);
    }

    [Test]
    public async Task TestParameterDefaultReturn()
    {
        using var script = await ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Input = 5
  results
    number Result
  Result = Input");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public async Task TestAssignmentReturn()
    {
        using var script = await ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Input = 5

  results
    number Result
  Result = Input");
        var result = script.Run(new Dictionary<string, object>
        {
            { "Input", 777 }
        });
        result.Number("Result").ShouldBe(777);
    }

    [Test]
    public async Task TestMemberAccessAssignment()
    {
        using var script = await ServiceProvider.CompileFunction(@"table ValidateTableKeyword
// Validate table keywords
  | number Value | number Result |
  | 0 | 0 |
  | 1 | 1 |

function ValidateTableKeywordFunction
// Validate table keywords
  parameters
  results
    number Result
  Result = ValidateTableKeyword.RowsCount");

        var result = script.Run();
        result.Number("Result").ShouldBe(2);
    }

    [Test]
    public async Task VariableDeclarationInCode()
    {
        using var script = await ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Value = 5 
  results
    number Result
  number temp = 5
  temp = Value 
  Result = temp");

        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public async Task VariableDeclarationWithDefaultInCode()
    {
        using var script = await ServiceProvider.CompileFunction(@"function TestSimpleReturn
  results
    number Result
  number temp = 5
  Result = temp
");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public async Task VariableDeclarationWithDefaultEnumInCode()
    {
        using var script = await ServiceProvider.CompileFunction(@"
enum SimpleEnum
  First
  Second
    
function TestSimpleReturn
  results
    SimpleEnum Result
  Result = SimpleEnum.Second
");
        var result = script.Run();
        result.GetValue("Result").ToString().ShouldBe("Second");
    }

    [Test]
    public async Task CustomType()
    {
        using var script = await ServiceProvider.CompileFunction(@"
type SimpleObject
  number First
  string Second
    
function TestCustomType
  results
    SimpleObject Result
  Result.First = 777
  Result.Second = ""123""
");
        var result = script.Run();
        var value = result.GetValue("Result") as dynamic;
        ((int) value.First).ShouldBe(777);
        ((string)value.Second).ShouldBe("123");
    }

    [Test]
    public async Task CustomTypeNestedProperties()
    {
        using var script = await ServiceProvider.CompileFunction(@"
type InnerObject
  number First
  string Second
    
type SimpleObject
  InnerObject Inner
    
function TestCustomType
  results
    SimpleObject Result
  Result.Inner.First = 777
  Result.Inner.Second = ""123""
");
        var result = script.Run();
        var value = result.GetValue("Result") as dynamic;
        ((int) value.Inner.First).ShouldBe(777);
        ((string) value.Inner.Second).ShouldBe("123");
    }
}
