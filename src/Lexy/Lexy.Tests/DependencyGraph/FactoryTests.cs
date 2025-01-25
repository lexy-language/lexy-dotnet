using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using NUnit.Framework;
using Shouldly;

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

    private const string function = @"Function: SimpleFunction
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
        dependencies.Nodes.Count.ShouldBe(1);
        dependencies.Nodes[0].Name.ShouldBe("SimpleEnum");
        dependencies.Nodes[0].Type.ShouldBe(typeof(EnumDefinition));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void SimpleTable()
    {
        var dependencies = ServiceProvider.BuildGraph(table);
        dependencies.Nodes.Count.ShouldBe(1);
        dependencies.Nodes[0].Name.ShouldBe("SimpleTable");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Table));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void SimpleFunction()
    {
        var dependencies = ServiceProvider.BuildGraph(function);
        dependencies.Nodes.Count.ShouldBe(1);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void FunctionNewFunctionParameters()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
Function: Caller
  Code
    var parameters = new(SimpleFunction.Parameters)
");

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Caller");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Dependencies[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[1].Dependencies[0].Type.ShouldBe(typeof(Function));
    }

    [Test]
    public void FunctionNewFunctionResults()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
Function: Caller
  Code
    var parameters = new(SimpleFunction.Results)
");

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Caller");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Dependencies[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[1].Dependencies[0].Type.ShouldBe(typeof(Function));
    }

    [Test]
    public void FunctionFillFunctionParameters()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
Function: Caller
  Parameters
    number Value
  Code
    var parameters = fill(SimpleFunction.Parameters)
");

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Caller");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Dependencies[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[1].Dependencies[0].Type.ShouldBe(typeof(Function));
    }

    [Test]
    public void FunctionFillFunctionResults()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"
Function: Caller
  Parameters
    number Result
  Code
    var parameters = fill(SimpleFunction.Results)
");

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Caller");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Dependencies[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[1].Dependencies[0].Type.ShouldBe(typeof(Function));
    }

    [Test]
    public void TableLookup()
    {
        var dependencies = ServiceProvider.BuildGraph(table + @"
Function: Caller
  Code
    var result = LOOKUP(SimpleTable, 2, SimpleTable.Search, SimpleTable.Value)
");

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleTable");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Table));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Caller");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Dependencies[0].Name.ShouldBe("SimpleTable");
        dependencies.Nodes[1].Dependencies[0].Type.ShouldBe(typeof(Table));
    }

    [Test]
    public void SimpleScenario()
    {
        var dependencies = ServiceProvider.BuildGraph(function + @"

Scenario: Simple
  Function SimpleFunction
  Results
    Result = 2
  Parameters
    Value = 2
");
        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("SimpleFunction");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Simple");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Scenario));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(0);
    }

    [Test]
    public void SimpleType()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
Type: Simple
  number Value1
  string Value2
");
        dependencies.Nodes.Count.ShouldBe(1);
        dependencies.Nodes[0].Name.ShouldBe("Simple");
        dependencies.Nodes[0].Type.ShouldBe(typeof(TypeDefinition));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
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
        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("Inner");
        dependencies.Nodes[0].Type.ShouldBe(typeof(TypeDefinition));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(0);
        dependencies.Nodes[1].Name.ShouldBe("Parent");
        dependencies.Nodes[1].Type.ShouldBe(typeof(TypeDefinition));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
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
        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("Inner");
        dependencies.Nodes[0].Type.ShouldBe(typeof(TypeDefinition));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Name.ShouldBe("Parent");
        dependencies.Nodes[1].Type.ShouldBe(typeof(TypeDefinition));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.CircularReferences.Count().ShouldBe(2);
        dependencies.CircularReferences[0].NodeName.ShouldBe("Inner");
        dependencies.CircularReferences[1].NodeName.ShouldBe("Parent");
    }

    [Test]
    public void CircularFunctionCall()
    {
        var dependencies = ServiceProvider.BuildGraph(@"
Function: Inner
  Code
    Parent()

Function: Parent
  Code
    Inner()
", false);

        dependencies.Nodes.Count.ShouldBe(2);
        dependencies.Nodes[0].Name.ShouldBe("Inner");
        dependencies.Nodes[0].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[0].Dependencies.Count.ShouldBe(1);
        dependencies.Nodes[1].Name.ShouldBe("Parent");
        dependencies.Nodes[1].Type.ShouldBe(typeof(Function));
        dependencies.Nodes[1].Dependencies.Count.ShouldBe(1);
        dependencies.CircularReferences.Count().ShouldBe(2);
        dependencies.CircularReferences[0].NodeName.ShouldBe("Inner");
        dependencies.CircularReferences[1].NodeName.ShouldBe("Parent");
    }
}