using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class ExpressionFactory : IExpressionFactory
{
    private static readonly IDictionary<Func<TokenList, bool>, Func<ExpressionSource, IExpressionFactory, ParseExpressionResult>>
        factories =
            new Dictionary<Func<TokenList, bool>, Func<ExpressionSource, IExpressionFactory, ParseExpressionResult>>
            {
                { IfExpression.IsValid, IfExpression.Parse },
                { ElseExpression.IsValid, ElseExpression.Parse },
                { ElseIfExpression.IsValid, ElseIfExpression.Parse },
                { SwitchExpression.IsValid, SwitchExpression.Parse },
                { CaseExpression.IsValid, CaseExpression.Parse },
                { VariableDeclarationExpression.IsValid, VariableDeclarationExpression.Parse },
                { AssignmentExpression.IsValid, AssignmentExpression.Parse },
                { ParenthesizedExpression.IsValid, ParenthesizedExpression.Parse },
                { BracketedExpression.IsValid, BracketedExpression.Parse },
                { IdentifierExpression.IsValid, IdentifierExpression.Parse },
                { MemberAccessExpression.IsValid, MemberAccessExpression.Parse },
                { LiteralExpression.IsValid, LiteralExpression.Parse },
                { BinaryExpression.IsValid, BinaryExpression.Parse },
                { FunctionCallExpression.IsValid, FunctionCallExpressionParser.Parse }
            };

    public ParseExpressionResult Parse(TokenList tokens, Line currentLine)
    {
        foreach (var factory in factories)
            if (factory.Key(tokens))
            {
                var source = new ExpressionSource(currentLine, tokens);
                return factory.Value(source, this);
            }

        return ParseExpressionResult.Invalid<Expression>($"Invalid expression: {tokens}");
    }
}