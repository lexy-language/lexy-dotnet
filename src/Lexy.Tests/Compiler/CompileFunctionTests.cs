using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public void TestSimpleReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"function TestSimpleReturn
  results
    number Result
  Code
    Result = 777");
        var result = script.Run();
        result.Number("Result").ShouldBe(777);
    }

    [Test]
    public void TestParameterDefaultReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Input = 5
  results
    number Result
  Code
    Result = Input");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public void TestAssignmentReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Input = 5

  results
    number Result
  Code
    Result = Input");
        var result = script.Run(new Dictionary<string, object>
        {
            { "Input", 777 }
        });
        result.Number("Result").ShouldBe(777);
    }


    [Test]
    public void TestMemberAccessAssignment()
    {
        using var script = ServiceProvider.CompileFunction(@"table ValidateTableKeyword
// Validate table keywords
  | number Value | number Result |
  | 0 | 0 |
  | 1 | 1 |

function ValidateTableKeywordFunction
// Validate table keywords
  parameters
  results
    number Result
  Code
    Result = ValidateTableKeyword.Count");

        var result = script.Run();
        result.Number("Result").ShouldBe(2);
    }

    [Test]
    public void VariableDeclarationInCode()
    {
        using var script = ServiceProvider.CompileFunction(@"function TestSimpleReturn
  parameters
    number Value = 5 
  results
    number Result
  Code
    number temp = 5
    temp = Value 
    Result = temp");

        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public void VariableDeclarationWithDefaultInCode()
    {
        using var script = ServiceProvider.CompileFunction(@"function TestSimpleReturn
  results
    number Result
  Code
    number temp = 5
    Result = temp
");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }


    [Test]
    public void VariableDeclarationWithDefaultEnumInCode()
    {
        using var script = ServiceProvider.CompileFunction(@"
enum SimpleEnum
  First
  Second
    
function TestSimpleReturn
  results
    SimpleEnum Result
  Code
    Result = SimpleEnum.Second
");
        var result = script.Run();
        result.GetValue("Result").ToString().ShouldBe("Second");
    }

    [Test]
    public void CustomType()
    {
        using var script = ServiceProvider.CompileFunction(@"
type SimpleComplex
  number First
  string Second
    
function TestCustomType
  results
    SimpleComplex Result
  Code
    Result.First = 777
    Result.Second = ""123""
");
        var result = script.Run();
        var value = result.GetValue("Result") as dynamic;
        ((int) value.First).ShouldBe(777);
        ((string)value.Second).ShouldBe("123");
    }

    [Test]
    public void CustomTypeNestedProperties()
    {
        using var script = ServiceProvider.CompileFunction(@"
type InnerComplex
  number First
  string Second
    
type SimpleComplex
  InnerComplex Inner
    
function TestCustomType
  results
    SimpleComplex Result
  Code
    Result.Inner.First = 777
    Result.Inner.Second = ""123""
");
        var result = script.Run();
        var value = result.GetValue("Result") as dynamic;
        ((int) value.Inner.First).ShouldBe(777);
        ((string) value.Inner.Second).ShouldBe("123");
    }
}