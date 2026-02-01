using System;
using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Tests;

public class VerifyContext
{
    private readonly VerifyLogging logging;

    public VerifyContext(VerifyLogging logging)
    {
        this.logging = Assert.NotNull(logging, nameof(logging));
    }

    public VerifyContext Collection<TItem>(IReadOnlyList<TItem> list, Action<VerifyCollectionContext<TItem>> testHandler)
        where TItem : class
    {
        Assert.NotNull(list, nameof(list));
        Assert.NotNull(testHandler, nameof(testHandler));

        var verify = new VerifyCollectionContext<TItem>(list, logging);
        testHandler(verify);

        return this;
    }

    public VerifyContext Fail(string message)
    {
        logging.LogAssert(false, message, "Failed");
        return this;
    }

    public VerifyContext IsTrue(bool contains, string message)
    {
        logging.LogAssert(contains, message, $"- Is true invalid: ");
        return this;
    }
}
