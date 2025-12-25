using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.DependencyGraph;

public class DependencyOrderTests : ScopedServicesTestFixture
{
    [Test]
    public void FunctionWithEnumAndTableDependency()
    {
        var dependencies = ServiceProvider.BuildGraph(
            @"function FunctionWithEnumDependency
  Parameters
    EnumExample EnumValue
  Results
    number Result
  Code
    Result = LOOKUP(TableExample, EnumExample.Single, TableExample.Example, TableExample.Value)

Table: TableExample
  | EnumExample Example | number Value |
  | EnumExample.Single  | 123          |

enum EnumExample
  Single
  Married
  CivilPartnership", false);

        dependencies.SortedNodes.Count.ShouldBe(3);
        dependencies.SortedNodes[0].NodeName.ShouldBe("EnumExample");
        dependencies.SortedNodes[1].NodeName.ShouldBe("TableExample");
        dependencies.SortedNodes[2].NodeName.ShouldBe("FunctionWithEnumDependency");
        dependencies.CircularReferences.Count.ShouldBe(0);
    }

    [Test]
    public void ComplexDependencyGraph()
    {
        var dependencies = ServiceProvider.BuildGraph(
            @"scenario ValidateBuiltOrder
  function
    Parameters
      TypeExample Example
    Results
      number Result
    Code
      FunctionWithFunctionDependency()
      FunctionWithFunctionTypeDependency()
  Parameters
    Example.EnumValue = EnumExample.Single
    Example.Nested.EnumValue = EnumExample.Married
  Results
    Result = 777

function FunctionWithFunctionDependency
  Parameters
    TypeExample Example
  Results
    number Result
  Code
    FunctionWithTypeDependency()
    FunctionWithTableDependency()
    FunctionWithEnumDependency()

function FunctionWithFunctionTypeDependency
  Parameters
    TypeExample Example
  Results
    number Result
  Code
    var functionParametersFill = fill(FunctionWithTypeDependency.Parameters)
    var functionParametersNew = new(FunctionWithTypeDependency.Parameters)
    var tableParameters = new(TableExample.Row)
    Result = 777

function FunctionWithTypeDependency
  Parameters
    TypeExample Example
  Results
    number Result
  Code
    Result = Example.Nested.Result

function FunctionWithTableDependency
  Parameters
    TypeExample Example
  Results
    number Result
  Code
    Result = LOOKUP(TableExample, EnumExample.Single, TableExample.Example, TableExample.Value)

function FunctionWithEnumDependency
  Parameters
    EnumExample EnumValue
    TypeExample Example
  Results
    number Result
  Code
    Result = 666

Type: NestedType
  EnumExample EnumValue
  number Result = 888

Type: TypeExample
  EnumExample EnumValue
  NestedType Nested

Table: TableExample
  | EnumExample Example | number Value |
  | EnumExample.Single  | 123          |

enum EnumExample
  Single
  Married
  CivilPartnership", true);

        dependencies.SortedNodes.Count.ShouldBe(11);
        dependencies.SortedNodes[0].NodeName.ShouldBe("NestedType");
        dependencies.SortedNodes[1].NodeName.ShouldBe("EnumExample");
        dependencies.SortedNodes[2].NodeName.ShouldBe("TypeExample");
        dependencies.SortedNodes[3].NodeName.ShouldBe("TableExample");
        dependencies.SortedNodes[4].NodeName.ShouldBe("FunctionWithTypeDependency");
        dependencies.SortedNodes[5].NodeName.ShouldBe("FunctionWithEnumDependency");
        dependencies.SortedNodes[6].NodeName.ShouldBe("FunctionWithTableDependency");
        dependencies.SortedNodes[7].NodeName.ShouldBe("FunctionWithFunctionTypeDependency");
        dependencies.SortedNodes[8].NodeName.ShouldBe("FunctionWithFunctionDependency");
        dependencies.SortedNodes[9].NodeName.ShouldBe("ValidateBuiltOrderFunction");
        dependencies.SortedNodes[10].NodeName.ShouldBe("ValidateBuiltOrder");
        dependencies.CircularReferences.Count.ShouldBe(0);
    }
}