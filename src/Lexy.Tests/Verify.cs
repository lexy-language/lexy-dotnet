using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexy.Tests;

public static class Verify
{
    public static void All(Action<VerifyContext> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyContext(logging);
        testHandler(verify);
        logging.AssertNoErrors();
    }

    public static async Task All(Func<VerifyContext, Task> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyContext(logging);
        await testHandler(verify);
        logging.AssertNoErrors();
    }

    public static void Model<TModel>(TModel model, Action<VerifyModelContext<TModel>> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyModelContext<TModel>(model, logging);
        testHandler(verify);
        logging.AssertNoErrors();
    }

    public static void Collection<TItem>(IReadOnlyList<TItem> list, Action<VerifyCollectionContext<TItem>> testHandler)
        where TItem : class
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyCollectionContext<TItem>(list, logging);
        testHandler(verify);
        logging.AssertNoErrors();
    }

    public static void ComparableCollection<TItem>(IReadOnlyList<TItem> list, Action<VerifyComparableCollectionContext<TItem>> testHandler)
        where TItem : class, IComparable
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var logging = new VerifyLogging();
        var verify = new VerifyComparableCollectionContext<TItem>(list, logging);
        testHandler(verify);
        logging.AssertNoErrors();
    }
}
