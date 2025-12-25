using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using NUnit.Framework;
using Shouldly;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Tests.DependencyGraph;

public class FactoryTests : ScopedServicesTestFixture
{
    private const string enumDefinition = @"Enum: SimpleEnum
  First
  Second
";

    private const string table = @"Table: SimpleTable
  | number Search | string Value |
  | 0 | ""0"" |
  | 1 | ""1"" |
  | 2 | ""2"" |
";

    private const string function = @"function SimpleFunction
  Parameters
    number Value
  Results
    number Result
  Code
    Result = Value
";

    [Test]
    public void SimpleEnum()
    {
        var dependencies = ServiceProvider.BuildGraph(enumDefinition);
        dependencies.DependencyNodes.Count.ShouldBe(1);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleEnum");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void SimpleTable()
    {
        var dependencies = ServiceProvider.BuildGraph(table);
        dependencies.DependencyNodes.Count.ShouldBe(1);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleTable");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void SimpleFunction()
    {
        var dependencies = ServiceProvider.BuildGraph(function);
        dependencies.DependencyNodes.Count.ShouldBe(1);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void FunctionNewFunctionParameters()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
function Caller
  Code
    var parameters = new(SimpleFunction.Parameters)
");

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Caller");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Dependencies[0].ShouldBe("SimpleFunction");
    }

    [Test]
    public void FunctionNewFunctionResults()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
function Caller
  Code
    var parameters = new(SimpleFunction.Results)
");

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Caller");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Dependencies[0].ShouldBe("SimpleFunction");
    }

    [Test]
    public void FunctionFillFunctionParameters()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
function Caller
  Parameters
    number Value
  Code
    var parameters = fill(SimpleFunction.Parameters)
");

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Caller");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Dependencies[0].ShouldBe("SimpleFunction");
    }

    [Test]
    public void FunctionFillFunctionResults()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
function Caller
  Parameters
    number Result
  Code
    var parameters = fill(SimpleFunction.Results)
");

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Caller");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Dependencies[0].ShouldBe("SimpleFunction");
    }

    [Test]
    public void TableLookup()
    {
        var dependencies = ServiceProvider.BuildGraph(table + @"
function Caller
  Code
    var result = LOOKUP(SimpleTable, 2, SimpleTable.Search, SimpleTable.Value)
");

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleTable");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Caller");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Dependencies[0].ShouldBe("SimpleTable");
    }

    [Test]
    public void SimpleScenario()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"

Scenario: Simple
  function SimpleFunction
  Results
    Result = 2
  Parameters
    Value = 2
");
        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Simple");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
    }

    [Test]
    public void SimpleType()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
Type: Simple
  number Value1
  string Value2
");
        dependencies.DependencyNodes.Count.ShouldBe(1);
        dependencies.DependencyNodes[0].Name.ShouldBe("Simple");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void ComplexType()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
Type: Inner
  number Value1
  string Value2

Type: Parent
  number Value1
  string Value2
  Inner Value3
");
        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("Inner");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.DependencyNodes[1].Name.ShouldBe("Parent");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
    }

    [Test]
    public void CircularType()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
Type: Inner
  number Value1
  string Value2
  Parent Value3

Type: Parent
  number Value1
  string Value2
  Inner Value3
", false);
        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("Inner");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Name.ShouldBe("Parent");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.CircularReferences.Count().ShouldBe(2);
        dependencies.CircularReferences[0].NodeName.ShouldBe("Inner");
        dependencies.CircularReferences[1].NodeName.ShouldBe("Parent");
    }

    [Test]
    public void CircularFunctionCall()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
function Inner
  Code
    Parent()

function Parent
  Code
    Inner()
", false);

        dependencies.DependencyNodes.Count.ShouldBe(2);
        dependencies.DependencyNodes[0].Name.ShouldBe("Inner");
        dependencies.DependencyNodes[0].Dependencies.Count.ShouldBe(1);
        dependencies.DependencyNodes[1].Name.ShouldBe("Parent");
        dependencies.DependencyNodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.CircularReferences.Count().ShouldBe(2);
        dependencies.CircularReferences[0].NodeName.ShouldBe("Inner");
        dependencies.CircularReferences[1].NodeName.ShouldBe("Parent");
    }
}