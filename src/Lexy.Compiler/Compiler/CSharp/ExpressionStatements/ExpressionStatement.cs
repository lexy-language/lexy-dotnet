using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

internal static class ExpressionStatement
{
    private record Entry(Func<Expression, bool> Matches, Func<Expression, IEnumerable<StatementSyntax>> Create);

    private static readonly IList<Entry> creators = new List<Entry>();

    static ExpressionStatement()
    {
        Add<VariableDeclarationExpression>(NewFunctionStatement.Matches, NewFunctionStatement.Create);
        Add<VariableDeclarationExpression>(FillFunctionStatement.Matches, FillFunctionStatement.Create);
        Add<ExtractResultsFunctionExpression>(ExtractFunctionStatement.Matches, ExtractFunctionStatement.Create);
        Add<LexyFunctionCallExpression>(AutoMapLexyFunctionStatement.Matches, AutoMapLexyFunctionStatement.Create);
    }

    private static void Add<TExpression>(Func<TExpression, bool> matches, Func<TExpression, IEnumerable<StatementSyntax>> create)
        where TExpression : Expression
    {
        bool Matches(Expression expression)
        {
            return expression is TExpression specific && matches(specific);
        }

        IEnumerable<StatementSyntax> Create(Expression expression)
        {
            if (expression is not TExpression specific)
            {
                throw new InvalidOperationException(
                    $"Expression is of type {expression.GetType()} but should be of type '{typeof(TExpression)}'.");
            }

            return create(specific);
        };

        creators.Add(new Entry(Matches, Create));
    }

    public static Func<Expression, IEnumerable<StatementSyntax>> GetCreator(Expression expression)
    {
        foreach (var rule in creators)
        {
            if (rule.Matches(expression))
            {
                return rule.Create;
            }
        }
        return null;
    }
}