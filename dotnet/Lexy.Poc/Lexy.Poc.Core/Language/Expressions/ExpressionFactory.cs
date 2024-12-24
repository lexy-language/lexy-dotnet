using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public static class ExpressionFactory
    {
        private static readonly IDictionary<Func<TokenList, bool>, Func<ExpressionSource, ParseExpressionResult>> factories =
            new Dictionary<Func<TokenList, bool>, Func<ExpressionSource, ParseExpressionResult>>()
            {
                { IfExpression.IsValid, IfExpression.Parse },
                { ElseExpression.IsValid, ElseExpression.Parse },
                { VariableDeclarationExpression.IsValid, VariableDeclarationExpression.Parse },
                { AssignmentExpression.IsValid, AssignmentExpression.Parse },
                { ParenthesizedExpression.IsValid, ParenthesizedExpression.Parse },
                { BracketedExpression.IsValid, BracketedExpression.Parse },
                { VariableExpression.IsValid, VariableExpression.Parse },
                { MemberAccessExpression.IsValid, MemberAccessExpression.Parse },
                { LiteralExpression.IsValid, LiteralExpression.Parse },
                { BinaryExpression.IsValid, BinaryExpression.Parse },
                { FunctionCallExpression.IsValid, FunctionCallExpression.Parse },
            };

        public static Expression Parse(SourceFile file, TokenList tokens, Line currentLine)
        {
            foreach (var factory in factories)
            {
                if (factory.Key(tokens))
                {
                    var source = new ExpressionSource(file, currentLine, tokens);
                    var expressionResult = factory.Value(source);
                    switch (expressionResult.Status)
                    {
                        case ParseExpressionStatus.Success:
                            return expressionResult.Expression;
                        case ParseExpressionStatus.Failed:
                            throw new InvalidOperationException($"Invalid expression: {tokens}. {expressionResult.ErrorMessage}");
                    }
                }
            }

            throw new InvalidOperationException($"Invalid expression: {tokens}");
        }
    }
}