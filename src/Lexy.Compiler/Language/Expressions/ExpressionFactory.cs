using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public static class ExpressionFactory
{
    private record Entry(Func<TokenList, bool> IsValid, Func<ExpressionSource, NodeReference, ParseExpressionResult> Parse);

    private static readonly IList<Entry> Factories =
        new List<Entry>
        {
            new (IfExpression.IsValid, IfExpression.Parse),
            new (ElseExpression.IsValid, ElseExpression.Parse),
            new (ElseIfExpression.IsValid, ElseIfExpression.Parse),
            new (SwitchExpression.IsValid, SwitchExpression.Parse),
            new (CaseExpression.IsValid, CaseExpression.Parse),
            new (VariableDeclarationExpression.IsValid, VariableDeclarationExpression.Parse),
            new (SpreadAssignmentExpression.IsValid, SpreadAssignmentExpression.Parse),
            new (AssignmentExpression.IsValid, AssignmentExpression.Parse),
            new (ParenthesizedExpression.IsValid, ParenthesizedExpression.Parse),
            new (BracketedExpression.IsValid, BracketedExpression.Parse),
            new (IdentifierExpression.IsValid, IdentifierExpression.Parse),
            new (MemberAccessExpression.IsValid, MemberAccessExpression.Parse),
            new (LiteralExpression.IsValid, LiteralExpression.Parse),
            new (SpreadExpression.IsValid, SpreadExpression.Parse),
            new (BinaryExpression.IsValid, BinaryExpression.Parse),
            new (FunctionCallExpression.IsValid, FunctionCallExpressionParser.Parse)
        };


    public static ParseExpressionResult Parse(INode parent, TokenList tokens, Line currentLine)
    {
        return Parse(new NodeReference(parent), tokens, currentLine);
    }

    public static ParseExpressionResult Parse(NodeReference parentReference, TokenList tokens, Line currentLine)
    {
        foreach (var factory in Factories)
        {
            if (factory.IsValid(tokens))
            {
                var source = new ExpressionSource(currentLine, tokens);
                return factory.Parse(source, parentReference);
            }
        }

        return ParseExpressionResult.Invalid<Expression>($"Invalid expression: {tokens}");
    }
}
