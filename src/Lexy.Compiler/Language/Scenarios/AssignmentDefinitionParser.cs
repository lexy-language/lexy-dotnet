using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public static class AssignmentDefinitionParser
{
    public static IAssignmentDefinition Parse(IParseLineContext context, IdentifierPath parentVariable = null)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var reference = line.LineStartReference();

        var assignmentIndex = tokens.Find<OperatorToken>(token => token.Type == OperatorType.Assignment);
        if (assignmentIndex <= 0)
        {
            context.Logger.Fail(reference, "Invalid assignment. Expected: 'Variable = Value'");
            return null;
        }

        var targetTokens = tokens.TokensFromStart(assignmentIndex);
        if (parentVariable != null) {
            targetTokens = AddParentVariableAccessor(parentVariable, targetTokens);
        }
        var targetExpression = context.ExpressionFactory.Parse(targetTokens, line);
        if (context.Failed(targetExpression, reference)) return null;

        var variableReference = VariablePathExpressionParser.Parse(targetExpression.Result);
        if (context.Failed(variableReference, reference)) return null;

        if (assignmentIndex == tokens.Length - 1) {
            return new ComplexAssignmentDefinition(variableReference.Result, reference);
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
        var variablePath = GetVariablePath(targetTokens);
        if (variablePath == null) return targetTokens;

        var newPath = parentVariable.Append(variablePath.Parts).FullPath();
        var newToken = new MemberAccessLiteralToken(newPath, variablePath.FirstCharacter);
        return new TokenList(new Token[] {newToken});
    }
    private record TokenVariablePath(string[] Parts, TokenCharacter FirstCharacter);

    private static TokenVariablePath GetVariablePath(TokenList targetTokens)
    {
        return targetTokens[0] switch
        {
            MemberAccessLiteralToken memberAccess => new TokenVariablePath(memberAccess.Parts, memberAccess.FirstCharacter),
            StringLiteralToken literal => new TokenVariablePath(new[] { literal.Value }, literal.FirstCharacter),
            _ => null
        };
    }
}