using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public static class DuplicateChecker
{
    public static void Validate<T>(IValidationContext context, Func<T, SourceReference> getReference,
        Func<T, string> getName, Func<T, string> getErrorMessage, IEnumerable<T> values)
    {
        Assert.NotNull(context, nameof(context));
        Assert.NotNull(getReference, nameof(getReference));
        Assert.NotNull(getName, nameof(getName));
        Assert.NotNull(getErrorMessage, nameof(getErrorMessage));
        Assert.NotNull(values, nameof(values));

        var found = new List<string>();
        foreach (var item in values)
        {
            var name = getName(item);
            if (found.Contains(name))
            {
                context.Logger.Fail(getReference(item), getErrorMessage(item));
            }
            else
            {
                found.Add(name);
            }
        }
    }
}