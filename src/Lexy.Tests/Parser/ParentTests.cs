using System;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using NUnit.Framework;

namespace Lexy.Tests.Parser;

public class ParentTests : ScopedServicesTestFixture
{
    [Test]
    public async Task SimpleEnum()
    {
        const string code = @"scenario ValidateBuildOrder
  function
    parameters
      TypeExample Example
    results
      number Result
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
  ... = FunctionWithTypeDependency(...)
  ... = FunctionWithTableDependency(...)
  ... = FunctionWithEnumDependency(...)

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
  CivilPartnership";

        var result = await ServiceProvider.ParseNodes(code);

        Console.WriteLine(NodesLogger.Log(result.Nodes));

        Verify.All(context =>
            NodesWalker.Walk(result.Nodes, node => VerifyParentChildrenAreSet(node, context)));
    }

    private static void VerifyParentChildrenAreSet(INode node, VerifyContext context)
    {
        var parent = node.Parent;
        if (parent == null)
        {
            if (node is not LexyScriptNode)
            {
                context.Fail($"Node: {node.GetType().Name}.parent should not be null");
            }
        }
        else
        {
            var children = parent.GetChildren();
            var contains = children.Contains(node);

            context.IsTrue(contains, node.GetType().Name + " not found as child of " + parent.GetType().Name);
        }
    }
}
