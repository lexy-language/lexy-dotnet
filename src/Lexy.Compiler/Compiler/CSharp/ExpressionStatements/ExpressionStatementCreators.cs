using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

internal static class ExpressionStatementCreators
{
    private record Entry(Func<Expression, bool> Matches, Func<Expression, IEnumerable<StatementSyntax>> CreateExpressionSyntax);

    private static readonly IList<Entry> creators = new List<Entry>();

    static ExpressionStatementCreators()
    {
        AddCreator<NewFunctionStatementCreator, VariableDeclarationExpression>();
        AddCreator<FillFunctionStatementCreator, VariableDeclarationExpression>();
        AddCreator<ExtractFunctionStatementCreator, ExtractResultsFunction>();
        AddCreator<AutoMapLexyFunctionStatementCreator, LexyFunctionCallExpression>();
    }

    private static void AddCreator<TCreator, TExpression>()
        where TExpression : Expression
        where TCreator : IExpressionStatementCreator<TExpression>, new()
    {
        bool Matches(Expression expression)
        {
            var rule = new TCreator();
            return expression is TExpression specific && rule.Matches(specific);
        }

        IEnumerable<StatementSyntax> Create(Expression expression)
        {
            var rule = new TCreator();
            if (expression is not TExpression specific)
            {
                throw new InvalidOperationException(
                    $"Expression is of type {expression.GetType()} but should be of type {typeof(TExpression)}.");
            }

            return rule.CreateExpressionSyntax(specific);
        };

        creators.Add(new Entry(Matches, Create));
    }

    public static Func<Expression, IEnumerable<StatementSyntax>> CreateExpressionSyntax(Expression expression)
    {
        foreach (var rule in creators)
        {
            if (rule.Matches(expression))
            {
                return rule.CreateExpressionSyntax;
            }
        }
        return null;
    }
}