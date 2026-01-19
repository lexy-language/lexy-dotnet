using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public static class IdentifierPathExpressionParser
{
    public static IdentifierPathParseResult Parse(Expression expression)
    {
        return expression switch
        {
            MemberAccessExpression memberAccessExpression => Parse(memberAccessExpression),
            LiteralExpression literalExpression => Parse(literalExpression),
            IdentifierExpression literalExpression => IdentifierPathParseResult.Success(IdentifierPath.Parse(literalExpression.Identifier)),
            _ => IdentifierPathParseResult.Failed("Invalid constant value. Expected: 'Variable = ConstantValue'")
        };
    }

    private static IdentifierPathParseResult Parse(LiteralExpression literalExpression)
    {
        return literalExpression.Literal switch
        {
            StringLiteralToken stringLiteral => IdentifierPathParseResult.Success(IdentifierPath.Parse(stringLiteral.Value)),
            _ => IdentifierPathParseResult.Failed("Invalid expression literal. Expected: 'Variable = ConstantValue'")
        };
    }

    private static IdentifierPathParseResult Parse(MemberAccessExpression memberAccessExpression)
    {
        if (memberAccessExpression.MemberAccessLiteralToken.Parts.Length == 0)
            return IdentifierPathParseResult.Failed("Invalid number of variable reference parts: "
                                                  + memberAccessExpression.MemberAccessLiteralToken.Parts.Length);

        var variableReference = new IdentifierPath(memberAccessExpression.MemberAccessLiteralToken.Parts);
        return IdentifierPathParseResult.Success(variableReference);
    }
}
