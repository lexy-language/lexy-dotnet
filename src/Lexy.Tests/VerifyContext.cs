using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser.Symbols;
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

    public VerifyContext Log(string message)
    {
        logging.AppendLine(message);
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

    public VerifyContext IsNotNull<TSubModel>(TSubModel value, Action<VerifyModelContext<TSubModel>> subContext, string extraMessage = null)
    {
        var valid = value != null;
        if (valid)
        {
            return InContext(subContext, value);
        }
        logging.LogAssert(false, extraMessage, $"- IsNotNull Failed: ");
        return this;
    }

    public VerifyContext IsNull<TSubModel>(TSubModel value, string extraMessage = null)
    {
        logging.LogAssert(value == null, extraMessage, $"- IsNull Failed '{{0}}'", value);
        return this;
    }

    private VerifyContext InContext<TSubModel>(Action<VerifyModelContext<TSubModel>> subContext,
        TSubModel value)
    {
        logging.WithIndentation(() => subContext(new VerifyModelContext<TSubModel>(value, logging)));

        return this;
    }
}
