using System.Threading.Tasks;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetDocumentNodesInScopeTests : VerifySuggestionsFixture
{
    private const string TwoFunctionCode = @"function Example
  parameters
    number Value1
    number Value2
  results
    number Result1
    number Result2
  var a = 2 + 3
  var b = 4 + 5
  Result1 = a + b

function Example2
  parameters
    number Value3
    number Value4
  results
    number Result5
    number Result6
  var a2 = 22 + 23
  var b2 = 24 + 25
  Result5 = a2 + b2
";

    [Test]
    public async Task FunctionNodes()
    {
        var result = await ServiceProvider.GetSymbols($"test.lexy", TwoFunctionCode, true);

        var nodes = result.DocumentSymbols.GetNodesInScope(new Position(8, 4));
        Verify.Collection(nodes, _ => _
            .Length(6, "nodes.Length")
            .ValueAt(0, node => node is Function { Name: "Example" })
            .ValueAt(1, node => node is FunctionParameters)
            .ValueAt(2, node => node is FunctionResults)
            .ValueAt(3, node => node is FunctionCode)
            .ValueAt(4, node => node is VariableDeclarationExpression)
            .ValueAt(5, node => node is ImplicitTypeDeclaration)
        );
    }

    [Test]
    public async Task SecondFunctionKeyword()
    {
        var result = await ServiceProvider.GetSymbols($"test.lexy", TwoFunctionCode, true);
        var nodes = result.DocumentSymbols.GetNodesInScope(new Position(20, 4));
        Verify.Collection(nodes, _ => _
            .Length(7, "nodes.Length")
            .ValueAt(0, node => node is Function { Name: "Example2" })
            .ValueAt(1, node => node is FunctionParameters)
            .ValueAt(2, node => node is FunctionResults)
            .ValueAt(3, node => node is FunctionCode)
            .ValueAt(4, node => node is VariableDeclarationExpression)
            .ValueAt(5, node => node is VariableDeclarationExpression)
            .ValueAt(6, node => node is ImplicitTypeDeclaration)
        );
    }
}
