using System;
using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class SpreadExpression : Expression
{
    private SpreadExpression(ExpressionSource source, SourceReference reference) :
        base(source, reference)
    {
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<LiteralExpression>("Invalid expression.");

        var reference = source.CreateReference();

        var expression = new SpreadExpression( source, reference);
        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1 && tokens.IsOperatorToken(0, OperatorType.Spread);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Type DeriveType(IValidationContext context)
    {
        context.Logger.Fail(Reference, "Invalid spread operator. The spread operator '...' can only be used in an Lexy function call with as a single argument.");
        return null;
    }

    public override Symbol GetSymbol() => new Symbol(Reference, "spread operator", string.Empty, SymbolKind.Operator);
}
