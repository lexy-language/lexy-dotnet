using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;
using NUnit.Framework;

namespace Lexy.Tests.DependencyGraph;

public class FactoryTests : ScopedServicesTestFixture
{
    private const string enumDefinition = @"enum SimpleEnum
  First
  Second
";

    private const string table = @"table SimpleTable
  | number Search | string Value |
  | 0 | ""0"" |
  | 1 | ""1"" |
  | 2 | ""2"" |
";

    private const string function = @"function SimpleFunction
  parameters
    number Value
  results
    number Result
  Result = Value
";

    private readonly Expression<Func<IComponentNode,string>> nodeType = item => item.NodeName;
    private readonly Expression<Func<Dependencies,IReadOnlyList<IComponentNode>>> sortedNodes = value => value.SortedNodes;

    [Test]
    public async Task SimpleEnum()
    {
        var dependencies = await ServiceProvider.BuildGraph(enumDefinition);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 1)
            .ContainsKey(model => model.Nodes, "SimpleEnum", __ => __
                .AreEqual(simpleEnum => simpleEnum.Dependencies.Count, 0)
                .AreEqual(simpleEnum => simpleEnum.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleEnum")
        );
    }

    [Test]
    public async Task SimpleTable()
    {
        var dependencies = await ServiceProvider.BuildGraph(table);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 1)
            .ContainsKey(model => model.Nodes, "SimpleTable", __ => __
                .AreEqual(simpleEnum => simpleEnum.Dependencies.Count, 0)
                .AreEqual(simpleEnum => simpleEnum.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleTable")
        );
    }

    [Test]
    public async Task SimpleFunction()
    {
        var dependencies = await ServiceProvider.BuildGraph(function);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 1)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
        );
    }

    [Test]
    public async Task FunctionNewFunctionParameters()
    {
        var dependencies = await ServiceProvider.BuildGraph(function + @"
function Caller
  var params = new(SimpleFunction.Parameters)
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 1)
                .ContainsKey(simpleFunction => simpleFunction.Dependants, "Caller")
            )
            .ContainsKey(model => model.Nodes, "Caller", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleFunction")
                .AreEqual(caller => caller.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Caller")
        );
    }

    [Test]
    public async Task FunctionNewFunctionResults()
    {
        var dependencies = await ServiceProvider.BuildGraph(function + @"
function Caller
  var params = new(SimpleFunction.Results)
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 1)
                .ContainsKey(simpleFunction => simpleFunction.Dependants, "Caller")
            )
            .ContainsKey(model => model.Nodes, "Caller", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleFunction")
                .AreEqual(caller => caller.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Caller")
        );
    }

    [Test]
    public async Task FunctionFillFunctionParameters()
    {
        var dependencies = await ServiceProvider.BuildGraph(function + @"

function Caller
  parameters
    number Value
  var params = fill(SimpleFunction.Parameters)
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 1)
                .ContainsKey(simpleFunction => simpleFunction.Dependants, "Caller")
            )
            .ContainsKey(model => model.Nodes, "Caller", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleFunction")
                .AreEqual(caller => caller.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Caller")
        );
    }

    [Test]
    public async Task FunctionFillFunctionResults()
    {
        var dependencies = await ServiceProvider.BuildGraph(function + @"

function Caller
  parameters
    number Result
  var params = fill(SimpleFunction.Results)
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 1)
                .ContainsKey(simpleFunction => simpleFunction.Dependants, "Caller")
            )
            .ContainsKey(model => model.Nodes, "Caller", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleFunction")
                .AreEqual(caller => caller.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Caller")
        );
    }

    [Test]
    public async Task TableLookup()
    {
        var dependencies = await ServiceProvider.BuildGraph(table + @"
function Caller
  var result = SimpleTable.LookUp(2, SimpleTable.Search, SimpleTable.Value)
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleTable", __ => __
                .AreEqual(simpleTable => simpleTable.Dependencies.Count, 0)
                .AreEqual(simpleTable => simpleTable.Dependants.Count, 1)
                .ContainsKey(simpleTable => simpleTable.Dependants, "Caller")
            )
            .ContainsKey(model => model.Nodes, "Caller", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleTable")
                .AreEqual(caller => caller.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleTable")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Caller")
        );
    }

    [Test]
    public async Task SimpleScenario()
    {
        var dependencies = await ServiceProvider.BuildGraph(function + @"

scenario Simple
  function SimpleFunction
  results
    Result = 2
  parameters
    Value = 2
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "SimpleFunction", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 1)
                .ContainsKey(simpleFunction => simpleFunction.Dependants, "Simple")
            )
            .ContainsKey(model => model.Nodes, "Simple", __ => __
                .AreEqual(caller => caller.Dependencies.Count, 1)
                .ContainsKey(caller => caller.Dependencies, "SimpleFunction")
                .AreEqual(caller => caller.Dependencies.Count, 1)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "SimpleFunction")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Simple")
        );
    }

    [Test]
    public async Task SimpleType()
    {
        var dependencies = await ServiceProvider.BuildGraph(@"
type Simple
  number Value1
  string Value2
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 1)
            .ContainsKey(model => model.Nodes, "Simple", __ => __
                .AreEqual(simpleFunction => simpleFunction.Dependencies.Count, 0)
                .AreEqual(simpleFunction => simpleFunction.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "Simple")
        );
    }

    [Test]
    public async Task GeneratedType()
    {
        var dependencies = await ServiceProvider.BuildGraph(@"
type Inner
  number Value1
  string Value2

type Parent
  number Value1
  string Value2
  Inner Value3
");

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "Inner", __ => __
                .AreEqual(value => value.Dependencies.Count, 0)
                .AreEqual(value => value.Dependants.Count, 1)
                .ContainsKey(value => value.Dependants, "Parent")
            )
            .ContainsKey(model => model.Nodes, "Parent", __ => __
                .AreEqual(value => value.Dependencies.Count, 1)
                .ContainsKey(value => value.Dependencies, "Inner")
                .AreEqual(value => value.Dependants.Count, 0)
            )
            .ValueAtEquals(sortedNodes, 0, nodeType, "Inner")
            .ValueAtEquals(sortedNodes, 1, nodeType, "Parent")
        );
    }

    [Test]
    public async Task CircularType()
    {
        var dependencies = await ServiceProvider.BuildGraph(@"
type Inner
  number Value1
  string Value2
  Parent Value3

type Parent
  number Value1
  string Value2
  Inner Value3
", false);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "Inner", __ => __
                .AreEqual(value => value.Dependencies.Count, 1)
                .ContainsKey(value => value.Dependencies, "Parent")
                .AreEqual(value => value.Dependants.Count, 1)
                .ContainsKey(value => value.Dependants, "Parent")
            )
            .ContainsKey(model => model.Nodes, "Parent", __ => __
                .AreEqual(value => value.Dependencies.Count, 1)
                .ContainsKey(value => value.Dependencies, "Inner")
                .AreEqual(value => value.Dependants.Count, 1)
                .ContainsKey(value => value.Dependants, "Inner")
            )
            .CountIs(model => model.CircularReferences, 2)
            .ContainsKey(model => model.CircularReferences, "Inner")
            .ContainsKey(model => model.CircularReferences, "Parent")
            .AreEqual(model => model.SortedNodes.Count, 2)
        );
    }

    [Test]
    public async Task CircularFunctionCall()
    {
        var dependencies = await ServiceProvider.BuildGraph(@"
function Inner
  Parent()

function Parent
  Inner()
", false);

        Verify<Dependencies>.Model(dependencies, _ => _
            .CountIs(model => model.Nodes, 2)
            .ContainsKey(model => model.Nodes, "Inner", __ => __
                .AreEqual(inner => inner.Dependencies.Count, 1)
                .ContainsKey(inner => inner.Dependencies, "Parent")
                .AreEqual(inner => inner.Dependants.Count, 1)
                .ContainsKey(inner => inner.Dependants, "Parent")
            )
            .ContainsKey(model => model.Nodes, "Parent", __ => __
                .AreEqual(parent => parent.Dependencies.Count, 1)
                .ContainsKey(parent => parent.Dependencies, "Inner")
                .AreEqual(parent => parent.Dependants.Count, 1)
                .ContainsKey(parent => parent.Dependants, "Inner")
            )
            .CountIs(model => model.CircularReferences, 2)
            .ContainsKey(model => model.CircularReferences, "Inner")
            .ContainsKey(model => model.CircularReferences, "Parent")
            .AreEqual(model => model.SortedNodes.Count, 2)
        );
    }
}
