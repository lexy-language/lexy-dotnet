using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public static class VariablePathExpressionParser
{
    public static VariablePathParseResult Parse(Expression expression)
    {
        return expression switch
        {
            MemberAccessExpression memberAccessExpression => Parse(memberAccessExpression),
            LiteralExpression literalExpression => Parse(literalExpression),
            IdentifierExpression literalExpression => VariablePathParseResult.Success(IdentifierPath.Parse(literalExpression.Identifier)),
            _ => VariablePathParseResult.Failed("Invalid constant value. Expected: 'Variable = ConstantValue'")
        };
    }

    private static VariablePathParseResult Parse(LiteralExpression literalExpression)
    {
        return literalExpression.Literal switch
        {
            StringLiteralToken stringLiteral => VariablePathParseResult.Success(IdentifierPath.Parse(stringLiteral.Value)),
            _ => VariablePathParseResult.Failed("Invalid expression literal. Expected: 'Variable = ConstantValue'")
        };
    }

    private static VariablePathParseResult Parse(MemberAccessExpression memberAccessExpression)
    {
        if (memberAccessExpression.MemberAccessLiteralToken.Parts.Length == 0)
            return VariablePathParseResult.Failed("Invalid number of variable reference parts: "
                                                  + memberAccessExpression.MemberAccessLiteralToken.Parts.Length);

        var variableReference = new IdentifierPath(memberAccessExpression.MemberAccessLiteralToken.Parts);
        return VariablePathParseResult.Success(variableReference);
    }
}