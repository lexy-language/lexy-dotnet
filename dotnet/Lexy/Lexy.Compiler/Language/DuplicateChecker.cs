using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public static class DuplicateChecker
{
    public static void Validate<T>(IValidationContext context, Func<T, SourceReference> getReference,
        Func<T, string> getName, Func<T, string> getErrorMessage, IEnumerable<T> values)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (getReference == null) throw new ArgumentNullException(nameof(getReference));
        if (getName == null) throw new ArgumentNullException(nameof(getName));
        if (getErrorMessage == null) throw new ArgumentNullException(nameof(getErrorMessage));
        if (values == null) throw new ArgumentNullException(nameof(values));

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