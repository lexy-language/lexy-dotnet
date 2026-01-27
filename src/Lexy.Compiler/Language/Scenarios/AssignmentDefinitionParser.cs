using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public static class AssignmentDefinitionParser
{
    private record TokenIdentifierPath(string[] Parts, TokenCharacter FirstCharacter);

    public static IAssignmentDefinition Parse(IParseLineContext context, IdentifierPath parentVariable = null)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var reference = line.Tokens.AllReference();

        var assignmentIndex = tokens.Find<OperatorToken>(token => token.Type == OperatorType.Assignment);
        if (assignmentIndex <= 0)
        {
            context.Logger.Fail(reference, "Invalid assignment. Expected: 'Variable = Value'");
            return null;
        }

        var targetTokens = tokens.TokensFromStart(assignmentIndex);
        if (parentVariable != null)
        {
            targetTokens = AddParentVariableAccessor(parentVariable, targetTokens);
        }
        var targetExpression = context.ExpressionFactory.Parse(targetTokens, line);
        if (context.Failed(targetExpression, reference)) return null;

        var variableReference = IdentifierPathExpressionParser.Parse(targetExpression.Result);
        if (context.Failed(variableReference, reference)) return null;

        if (assignmentIndex == tokens.Length - 1)
        {
            return new ObjectAssignmentDefinition(variableReference.Result, reference);
        }

        var valueExpression = context.ExpressionFactory.Parse(tokens.TokensFrom(assignmentIndex + 1), line);
        if (context.Failed(valueExpression, reference)) return null;

        var constantValue = ConstantValue.Parse(valueExpression.Result);
        if (context.Failed(constantValue, reference)) return null;

        return new AssignmentDefinition(variableReference.Result, constantValue.Result, targetExpression.Result,
            valueExpression.Result, reference);
    }

    private static TokenList AddParentVariableAccessor(IdentifierPath parentVariable, TokenList targetTokens)
    {
        if (targetTokens.Length != 1) return targetTokens;
        var identifierPath = GetIdentifierPath(targetTokens);
        if (identifierPath == null) return targetTokens;

        var newPath = parentVariable.Append(identifierPath.Parts).FullPath();
        var newToken = new MemberAccessLiteralToken(newPath, identifierPath.FirstCharacter);
        return new TokenList(targetTokens.Line, new Token[] {newToken});
    }

    private static TokenIdentifierPath GetIdentifierPath(TokenList targetTokens)
    {
        return targetTokens[0] switch
        {
            MemberAccessLiteralToken memberAccess => new TokenIdentifierPath(memberAccess.Parts, memberAccess.FirstCharacter),
            StringLiteralToken literal => new TokenIdentifierPath(new[] { literal.Value }, literal.FirstCharacter),
            _ => null
        };
    }
}
