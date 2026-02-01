using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Symbols;
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
    public void FunctionNodes()
    {
        var symbols = ServiceProvider.GetSymbols($"test.lexy", TwoFunctionCode, true);
        var documentSymbols = symbols.Result.Symbols.Document("test.lexy");

        var nodes = documentSymbols.GetNodesInScope(new Position(8, 4));
        Verify.Collection(nodes, _ => _
            .Length(6, "nodes.Length")
            .ValueAt(0, node => node.Value is Function { Name: "Example" })
            .ValueAt(1, node => node.Value is FunctionParameters)
            .ValueAt(2, node => node.Value is FunctionResults)
            .ValueAt(3, node => node.Value is FunctionCode)
            .ValueAt(4, node => node.Value is VariableDeclarationExpression)
            .ValueAt(5, node => node.Value is ImplicitTypeDeclaration)
            .ValueAt(0, node => node.Level == 0)
            .ValueAt(1, node => node.Level == 1)
            .ValueAt(2, node => node.Level == 1)
            .ValueAt(3, node => node.Level == 1)
            .ValueAt(4, node => node.Level == 2)
            .ValueAt(5, node => node.Level == 3)
        );
    }

    [Test]
    public void SecondFunctionKeyword()
    {

      var symbols = ServiceProvider.GetSymbols($"test.lexy", TwoFunctionCode, true);
      var documentSymbols = symbols.Result.Symbols.Document("test.lexy");

      var nodes = documentSymbols.GetNodesInScope(new Position(20, 4));
      Verify.Collection(nodes, _ => _
        .Length(7, "nodes.Length")
        .ValueAt(0, node => node.Value is Function { Name: "Example2" })
        .ValueAt(1, node => node.Value is FunctionParameters)
        .ValueAt(2, node => node.Value is FunctionResults)
        .ValueAt(3, node => node.Value is FunctionCode)
        .ValueAt(4, node => node.Value is VariableDeclarationExpression)
        .ValueAt(5, node => node.Value is VariableDeclarationExpression)
        .ValueAt(6, node => node.Value is ImplicitTypeDeclaration)
        .ValueAt(0, node => node.Level == 0)
        .ValueAt(1, node => node.Level == 1)
        .ValueAt(2, node => node.Level == 1)
        .ValueAt(3, node => node.Level == 1)
        .ValueAt(4, node => node.Level == 2)
        .ValueAt(5, node => node.Level == 2)
        .ValueAt(6, node => node.Level == 3)
      );
    }
}
