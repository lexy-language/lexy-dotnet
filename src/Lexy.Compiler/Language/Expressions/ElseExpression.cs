using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class ElseExpression : Expression, IParsableNode, IChildExpression
{
    private readonly ExpressionList falseExpressions;

    public IEnumerable<Expression> FalseExpressions => falseExpressions;

    private ElseExpression(ExpressionSource source, NodeReference parentReference, SourceReference reference, IExpressionFactory factory) :
        base(source, parentReference, reference)
    {
        falseExpressions = new ExpressionList(this, reference, factory);
    }

    public override IEnumerable<INode> GetChildren()
    {
        return FalseExpressions;
    }

    public IParsableNode Parse(IParseLineContext context)
    {
        var expression = falseExpressions.Parse(context);
        return expression.Result is IParsableNode node ? node : this;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ElseExpression>("Not valid.");

        if (tokens.Length > 1) return ParseExpressionResult.Invalid<ElseExpression>("No tokens expected.");

        var reference = source.CreateReference();

        var expression = new ElseExpression(source, parentReference, reference, factory);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.Else);
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Type DeriveType(IValidationContext context)
    {
        return null;
    }

    public bool ValidateParentExpression(IParentExpression expression, IParseLineContext context)
    {
        if (expression is IfExpression) return true;

        context.Logger.Fail(Reference, "'else' should be following an 'if' statement. No 'if' statement found.");

        return false;
    }

    public override Symbol GetSymbol() => null;

    public override SuggestionEdit[] GetSuggestions()
    {
        return Suggestions.Edit(with => with
            .RemoveKeyword(Keywords.Else)
            .RemoveKeyword(Keywords.ElseIf)
        );
    }
}
