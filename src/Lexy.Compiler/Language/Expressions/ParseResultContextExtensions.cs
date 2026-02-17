using System;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

internal static class ParseResultContextExtensions
{
    internal static bool Failed<T>(this IParseLineContext context, ParseResult<T> result, SourceReference reference)
    {
        Assert.NotNull(context, nameof(context));
        Assert.NotNull(result, nameof(result));
        Assert.NotNull(reference, nameof(reference));

        if (result.IsSuccess) return false;

        context.Logger.Fail(reference, result.ErrorMessage);
        return true;
    }
}