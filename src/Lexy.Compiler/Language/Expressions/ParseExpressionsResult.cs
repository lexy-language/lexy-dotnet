using System.Collections.Generic;

namespace Lexy.Compiler.Language.Expressions;

public sealed class ParseExpressionsResult : ParseResult<IReadOnlyList<Expression>>
{
    private ParseExpressionsResult(IReadOnlyList<Expression> expression) : base(expression)
    {
    }

    private ParseExpressionsResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static ParseExpressionsResult Invalid<T>(string error)
    {
        return new ParseExpressionsResult(false, $"({typeof(T).Name}) {error}");
    }

    public static ParseExpressionsResult Success(IReadOnlyList<Expression> expression)
    {
        return new ParseExpressionsResult(expression);
    }
}