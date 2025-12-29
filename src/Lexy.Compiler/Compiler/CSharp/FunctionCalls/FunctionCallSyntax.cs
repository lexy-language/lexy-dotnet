using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.FunctionCalls;

internal static class FunctionCallSyntax
{
    private record Entry(
        Func<FunctionCallExpression, bool> Matches,
        Func<FunctionCallExpression, ExpressionSyntax> CreateExpressionSyntax);

    private static readonly IList<Entry> entries = new List<Entry>();

    static FunctionCallSyntax()
    {
        AddCreator<MemberFunctionCallExpression, TableLookUpFunctionCallCreator>();
        AddCreator<MemberFunctionCallExpression, TableLookUpRowFunctionCallCreator>();
        AddCreator<LexyFunctionCallExpression, LexyFunctionCallCreator>();
        AddCreator<MemberFunctionCallExpression, LibraryFunctionCallCreator>();
    }

    private static void AddCreator<TExpression, TCreator>()
        where TExpression : FunctionCallExpression
        where TCreator : IFunctionCallCreator<TExpression>, new()
    {
        bool Matches(FunctionCallExpression expression)
        {
            var creator = new TCreator();
            return expression is TExpression specific && creator.Matches(specific);
        }

        ExpressionSyntax Create(FunctionCallExpression expression)
        {
            var creator = new TCreator();
            var specific = CastExpression<TExpression>(expression);
            return creator.CreateExpressionSyntax(specific);
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