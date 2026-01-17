using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;
using NUnit.Framework;

namespace Lexy.Tests.DependencyGraph;

public class DependencyOrderTests : ScopedServicesTestFixture
{
    private readonly Expression<Func<IComponentNode, string>> nodeType = item => item.NodeName;

    private readonly Expression<Func<Dependencies, IReadOnlyList<IComponentNode>>> sortedNodes =
        value => value.SortedNodes;

    [Test]
    public async Task FunctionWithEnumAndTableDependency()
    {
        var dependencies = await ServiceProvider.BuildGraph(
            @"function FunctionWithEnumDependency
  parameters
    EnumExample EnumValue
  results
    number Result
  Result = TableExample.LookUp(EnumExample.Single, TableExample.Example, TableExample.Value)

table TableExample
  | EnumExample Example | number Value |
  | EnumExample.Single  | 123          |

enum EnumExample
  Single
  Married
  CivilPartnership", false);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 3)
            .ContainsKey(model => model.Nodes, "TableExample", __ => __
                .AreEqual(tableExample => tableExample.Dependencies.Count, 1)
                .ContainsKey(tableExample => tableExample.Dependencies, "EnumExample")
                .AreEqual(tableExample => tableExample.Dependants.Count, 1)
                .ContainsKey(tableExample => tableExample.Dependants, "FunctionWithEnumDependency")
            )
            .ContainsKey(model => model.Nodes, "EnumExample", __ => __
                .AreEqual(enumExample => enumExample.Dependencies.Count, 0)
                .AreEqual(enumExample => enumExample.Dependants.Count, 2)
                .ContainsKey(enumExample => enumExample.Dependants, "TableExample")
                .ContainsKey(enumExample => enumExample.Dependants, "FunctionWithEnumDependency")
            )
            .ContainsKey(model => model.Nodes, "FunctionWithEnumDependency", __ => __
                .AreEqual(functionWithEnumDependency => functionWithEnumDependency.Dependencies.Count, 2)
                .ContainsKey(functionWithEnumDependency => functionWithEnumDependency.Dependencies, "TableExample")
                .ContainsKey(functionWithEnumDependency => functionWithEnumDependency.Dependencies, "EnumExample")
                .AreEqual(functionWithEnumDependency => functionWithEnumDependency.Dependants.Count, 0)
            )
            .CountIs(sortedNodes, 3)
            .ValueAtEquals(sortedNodes, 0, nodeType, "EnumExample")
            .ValueAtEquals(sortedNodes, 1, nodeType, "TableExample")
            .ValueAtEquals(sortedNodes, 2, nodeType, "FunctionWithEnumDependency")
            .CountIs(value => value.CircularReferences, 0)
        );
    }

    [Test]
    public async Task ComplexDependencyGraph()
    {
        var dependencies = await ServiceProvider.BuildGraph(
            @"scenario ValidateBuildOrder
  function
    parameters
      TypeExample Example
    results
      number Result
      string Message
    ... = FunctionWithFunctionDependency(...)
    ... = FunctionWithFunctionTypeDependency(...)
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
    string Message
  ... = FunctionWithTypeDependency(...)
  ... = FunctionWithTableDependency(...)
  ... = FunctionWithEnumDependency(...)

function FunctionWithFunctionTypeDependency
  parameters
    TypeExample Example
  results
    number Result
    string Message
  var functionParametersFill = fill(FunctionWithTypeDependency.Parameters)
  var functionParametersNew = new(FunctionWithTypeDependency.Parameters)
  var tableParameters = new(TableExample.Row)
  Result = 777

function FunctionWithTypeDependency
  parameters
    TypeExample Example
  results
    number Result
    string Message
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
    string Message
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

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(value => value.SortedNodes, 11)
            .ValueAtEquals(sortedNodes, 0, nodeType, "EnumExample")
            .ValueAtEquals(sortedNodes, 1, nodeType, "NestedType")
            .ValueAtEquals(sortedNodes, 2, nodeType, "TypeExample")
            .ValueAtEquals(sortedNodes, 3, nodeType, "TableExample")
            .ValueAtEquals(sortedNodes, 4, nodeType, "FunctionWithTypeDependency")
            .ValueAtEquals(sortedNodes, 5, nodeType, "FunctionWithEnumDependency")
            .ValueAtEquals(sortedNodes, 6, nodeType, "FunctionWithTableDependency")
            .ValueAtEquals(sortedNodes, 7, nodeType, "FunctionWithFunctionTypeDependency")
            .ValueAtEquals(sortedNodes, 8, nodeType, "FunctionWithFunctionDependency")
            .ValueAtEquals(sortedNodes, 9, nodeType, "ValidateBuildOrderFunction")
            .ValueAtEquals(sortedNodes, 10, nodeType, "ValidateBuildOrder")
            .CountIs(value => value.CircularReferences, 0)
        );
    }
}
