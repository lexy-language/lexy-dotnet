using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Generation.CSharp.FunctionCalls;

internal static class FunctionCallSyntax
{
    private record Entry(
        Func<FunctionCallExpression, bool> Matches,
        Func<FunctionCallExpression, ExpressionSyntax> CreateExpressionSyntax);

    private static readonly IList<Entry> entries = new List<Entry>();

    static FunctionCallSyntax()
    {
        Add<MemberFunctionCallExpression>(TableLookUpFunctionCallSyntax.Matches, TableLookUpFunctionCallSyntax.Create);
        Add<MemberFunctionCallExpression>(TableLookUpRowFunctionCallSyntax.Matches, TableLookUpRowFunctionCallSyntax.Create);
        Add<LexyFunctionCallExpression>(LexyFunctionCallSyntax.Matches, LexyFunctionCallSyntax.Create);
        Add<MemberFunctionCallExpression>(LibraryFunctionCallSyntax.Matches, LibraryFunctionCallSyntax.Create);
    }

    private static void Add<TExpression>(Func<TExpression, bool> matches, Func<TExpression, ExpressionSyntax> create)
        where TExpression : FunctionCallExpression
    {
        bool Matches(FunctionCallExpression expression)
        {
            return expression is TExpression specific && matches(specific);
        }

        ExpressionSyntax Create(FunctionCallExpression expression)
        {
            var specific = CastExpression<TExpression>(expression);
            return create(specific);
        }

        entries.Add(new Entry(Matches, Create));
    }

    private static TExpression CastExpression<TExpression>(FunctionCallExpression expression)
        where TExpression : FunctionCallExpression
    {
        if (expression is not TExpression specific)
        {
            throw new InvalidOperationException(
                $"Expression is of type {expression.GetType()} but should be of type {typeof(TExpression)}.");
        }
        return specific;
    }


    public static ExpressionSyntax CreateExpressionSyntax(FunctionCallExpression expression)
    {
        foreach (var creator in entries)
        {
            if (creator.Matches(expression))
            {
                return creator.CreateExpressionSyntax(expression);
            }
        }
        throw new InvalidOperationException($"Couldn't find creator for expression: '{expression.GetType()}'");
    }
}