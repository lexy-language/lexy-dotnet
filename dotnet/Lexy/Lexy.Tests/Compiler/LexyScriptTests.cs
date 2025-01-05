using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class LexyScriptTests : ScopedServicesTestFixture
{
    [Test]
    public void TestSimpleReturn()
    {
        var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
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
        var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
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
        var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
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
        var script = ServiceProvider.CompileFunction(@"Table: ValidateTableKeyword
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
        var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
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
        var script = ServiceProvider.CompileFunction(@"Function: TestSimpleReturn
  Results
    number Result
  Code
    number temp = 5
    Result = temp
");
        var result = script.Run();
        result.Number("Result").ShouldBe(5);
    }
}