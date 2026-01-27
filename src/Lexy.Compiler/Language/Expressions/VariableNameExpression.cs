using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class VariableNameExpression : Expression
{
    private SymbolKind kind;

    public string Name { get; }

    private VariableNameExpression(string name, ExpressionSource source, SourceReference reference, SymbolKind kind) :
        base(source, reference)
    {
        Name = name;
        this.kind = kind;
    }

    public static ParseVariableNameExpressionResult Parse(ExpressionSource source, SymbolKind kind)
    {
        var expression = CreateExpression(source, source.Tokens, kind);
        return expression == null
            ? ParseVariableNameExpressionResult.Invalid<LiteralExpression>("Invalid expression.")
            : ParseVariableNameExpressionResult.Success(expression);
    }

    private static VariableNameExpression CreateExpression(ExpressionSource source, TokenList tokens, SymbolKind kind)
    {
        if (!IsValid(source.Tokens)) return null;

        var reference = source.CreateReference();

        var name = tokens.TokenValue(0);
        return new VariableNameExpression(name, source, reference, kind);
    }

    private static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1 && tokens.IsLiteralToken(0);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Type DeriveType(IValidationContext context) => null;

    public override Symbol GetSymbol() => new Symbol(Reference, Name, string.Empty, kind);
}
