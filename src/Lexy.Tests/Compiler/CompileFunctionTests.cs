using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public void TestSimpleReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Results
    number Result
  Code
    Result = 777");
        var result = script.Run();
        result.Number("Result").ShouldBe(777);
    }

    [Test]
    public void TestParameterDefaultReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Parameters
    number Input = 5
  Results
    number Result
  Code
    Result = Input");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }

    [Test]
    public void TestAssignmentReturn()
    {
        using var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Parameters
    number Input = 5

  Results
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
        using var script = ServiceProvider.CompileFunction(@"Table: ValidateTableKeyword
# Validate table keywords
  | number Value | number Result |
  | 0 | 0 |
  | 1 | 1 |

Function: ValidateTableKeywordFunction
# Validate table keywords
  Parameters
  Results
    number Result
  Code
    Result = ValidateTableKeyword.Count");

        var result = script.Run();
        result.Number("Result").ShouldBe(2);
    }

    [Test]
    public void VariableDeclarationInCode()
    {
        using var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Parameters
    number Value = 5 
  Results
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
        using var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Results
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
Enum: SimpleEnum
  First
  Second
    
Function: TestSimpleReturn
  Results
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
Type: SimpleComplex
  number First
  string Second
    
Function: TestCustomType
  Results
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
Type: InnerComplex
  number First
  string Second
    
Type: SimpleComplex
  InnerComplex Inner
    
Function: TestCustomType
  Results
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