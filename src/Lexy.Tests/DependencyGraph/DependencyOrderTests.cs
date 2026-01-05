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
  parameters
    EnumExample EnumValue
  results
    number Result
  Result = lookUp(TableExample, EnumExample.Single, TableExample.Example, TableExample.Value)

table TableExample
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
    parameters
      TypeExample Example
    results
      number Result
    FunctionWithFunctionDependency()
    FunctionWithFunctionTypeDependency()
  parameters
    Example.EnumValue = EnumExample.Single
    Example.Nested.EnumValue = EnumExample.Married
  results
    Result = 777

function FunctionWithFunctionDependency
  parameters
    TypeExample Example
  results
    number Result
  FunctionWithTypeDependency()
  FunctionWithTableDependency()
  FunctionWithEnumDependency()

function FunctionWithFunctionTypeDependency
  parameters
    TypeExample Example
  results
    number Result
  var functionParametersFill = fill(FunctionWithTypeDependency.Parameters)
  var functionParametersNew = new(FunctionWithTypeDependency.Parameters)
  var tableParameters = new(TableExample.Row)
  Result = 777

function FunctionWithTypeDependency
  parameters
    TypeExample Example
  results
    number Result
  Result = Example.Nested.Result

function FunctionWithTableDependency
  parameters
    TypeExample Example
  results
    number Result
  Result = TableExample.LookUp(EnumExample.Single, TableExample.Example, TableExample.Value)

function FunctionWithEnumDependency
  parameters
    EnumExample EnumValue
    TypeExample Example
  results
    number Result
  Result = 666

type NestedType
  EnumExample EnumValue
  number Result = 888

type TypeExample
  EnumExample EnumValue
  NestedType Nested

table TableExample
  | EnumExample Example | number Value |
  | EnumExample.Single  | 123          |

enum EnumExample
  Single
  Married
  CivilPartnership", true);

        dependencies.SortedNodes.Count.ShouldBe(11);
        dependencies.SortedNodes[0].NodeName.ShouldBe("EnumExample");
        dependencies.SortedNodes[1].NodeName.ShouldBe("NestedType");
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