using System;
using System.Collections.Generic;

namespace Lexy.Tests;

public class Verify
{
    public static void Model<TModel>(TModel model, Action<VerifyModelContext<TModel>> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyModelContext<TModel>(model, logging);
        testHandler(verify);
        VerifyAll(logging);
    }

    public static void Collection<TItem>(IReadOnlyList<TItem> list, Action<VerifyCollectionContext<TItem>> testHandler)
        where TItem : class, IComparable
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyCollectionContext<TItem>(list, logging);
        testHandler(verify);
        VerifyAll(logging);
    }

    private static void VerifyAll(VerifyLogging logging)
    {
        var summary = logging.ToString();
        if (logging.Errors > 0)
        {
            throw new InvalidOperationException(summary);
        }

        Console.WriteLine(summary);
    }
}
