using System;
using System.Linq.Expressions;

namespace Lexy.Tests;

public static class ExpressionCompilerExtensions
{
    public static (TReturn value, string message) CompileExpression<TValue, TReturn>(this Expression<Func<TValue, TReturn>> expression, TValue model)
    {
        var value = expression.Compile().Invoke(model);
        var message = GetPath(expression);
        return (value, message);
    }

    private static string GetPath(LambdaExpression expression)
    {
        return expression.Body.ToString();
    }
}