using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public static class AssignmentDefinitionParser
{
    private record TokenIdentifierPath(string[] Parts, TokenCharacter FirstCharacter);

    public static IAssignmentDefinition Parse(IParseLineContext context, INode parent, IdentifierPath parentVariable = null)
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

        var expressionReference = new NodeReference();
        var targetExpression = ParseTargetExpression(context, parentVariable, tokens, assignmentIndex, expressionReference, line);
        if (context.Failed(targetExpression, reference)) return null;

        var variableReference = IdentifierPathExpressionParser.Parse(targetExpression.Result);
        if (context.Failed(variableReference, reference)) return null;

        if (assignmentIndex == tokens.Length - 1)
        {
            var definition = new ObjectAssignmentDefinition(variableReference.Result, new NodeReference(parent), reference);
            expressionReference.SetNode(definition);
            return definition;
        }

        var valueExpression = ExpressionFactory.Parse(expressionReference, tokens.TokensFrom(assignmentIndex + 1), line);
        if (context.Failed(valueExpression, reference)) return null;

        var constantValue = ConstantValueParser.Parse(valueExpression.Result);
        if (context.Failed(constantValue, reference)) return null;

        var assignmentDefinition = new AssignmentDefinition(variableReference.Result, constantValue.Result, targetExpression.Result,
            valueExpression.Result, new NodeReference(parent), reference);
        expressionReference.SetNode(assignmentDefinition);
        return assignmentDefinition;
    }

    private static ParseExpressionResult ParseTargetExpression(IParseLineContext context, IdentifierPath parentVariable,
        TokenList tokens, int assignmentIndex, NodeReference expressionReference, Line line)
    {
        var targetTokens = tokens.TokensFromStart(assignmentIndex);
        if (parentVariable != null)
        {
            targetTokens = AddParentVariableAccessor(parentVariable, targetTokens);
        }

        return ExpressionFactory.Parse(expressionReference, targetTokens, line);
    }

    private static TokenList AddParentVariableAccessor(IdentifierPath parentVariable, TokenList targetTokens)
    {
        if (targetTokens.Length != 1) return targetTokens;
        var identifierPath = GetIdentifierPath(targetTokens);
        if (identifierPath == null) return targetTokens;

        var newPath = parentVariable.Append(identifierPath.Parts).FullPath();
        var newToken = new MemberAccessToken(newPath, identifierPath.FirstCharacter, targetTokens[0].EndColumn);
        return new TokenList(targetTokens.Line, newToken);
    }

    private static TokenIdentifierPath GetIdentifierPath(TokenList targetTokens)
    {
        return targetTokens[0] switch
        {
            MemberAccessToken memberAccess => new TokenIdentifierPath(memberAccess.Parts, memberAccess.FirstCharacter),
            StringLiteralToken literal => new TokenIdentifierPath(new[] { literal.Value }, literal.FirstCharacter),
            _ => null
        };
    }
}
