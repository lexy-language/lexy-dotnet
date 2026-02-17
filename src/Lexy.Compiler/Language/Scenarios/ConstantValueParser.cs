using Lexy.Compiler.Language.Expressions;

namespace Lexy.Compiler.Language.Scenarios;

public static class ConstantValueParser
{
    public static ConstantValueParseResult Parse(Expression expression)
    {
        return expression switch
        {
            LiteralExpression literalExpression => Parse(literalExpression),
            MemberAccessExpression literalExpression => Parse(literalExpression),
            _ => ConstantValueParseResult.Failed("Invalid expression variable. Expected: 'Variable = ConstantValue'")
        };
    }

    private static ConstantValueParseResult Parse(LiteralExpression literalExpression)
    {
        var value = new ConstantValue(literalExpression.Literal.TypedValue);
        return ConstantValueParseResult.Success(value);
    }

    private static ConstantValueParseResult Parse(MemberAccessExpression literalExpression)
    {
        return ConstantValueParseResult.Success(new ConstantValue(literalExpression.MemberAccessToken.Value));
    }
}