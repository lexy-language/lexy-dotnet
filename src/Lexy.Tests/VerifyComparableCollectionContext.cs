using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexy.Tests;

public class VerifyComparableCollectionContext<TItem> : VerifyModelContext<IReadOnlyList<TItem>>
    where TItem : class, IComparable
{
    public VerifyComparableCollectionContext(IReadOnlyList<TItem> model, VerifyLogging logging) : base(model, logging)
    {
    }

    public VerifyComparableCollectionContext<TItem> ValueAtEquals(int index, TItem expected)
    {
        var value = index >= 0 && index < Model.Count ? Model[index] : null;
        if (value != null)
        {
            Logging.LogAssert(expected.CompareTo(value) == 0, "collection", "- ValueAtEquals[{0}] '{1}' != '{2}': ", index, expected, value);
            return this;
        }

        Logging.LogAssert(false, "collection", "- ValueAtEquals[{0}] invalid: ", index);

        return this;
    }

    public VerifyComparableCollectionContext<TItem> Length(int length, string extraMessage)
    {
        var suffix = extraMessage != null ? $" ({extraMessage})" : "";
        Logging.LogAssert(Model.Count == length, nameof(Length), $"- Length Failed '{{0}}' != '{{1}}'{suffix}: ", Model, length);
        return this;
    }

    public VerifyComparableCollectionContext<TItem> Contains(TItem expected)
    {
        var value = Model.Contains(expected);

        Logging.LogAssert(value, "collection", "- Contains[{0}] invalid: ", expected);

        return this;
    }

    public VerifyComparableCollectionContext<TItem> Any(Func<TItem, bool> criteria)
    {
        var value = Model.Any(criteria);

        Logging.LogAssert(value, "collection", "- Any[{0}] invalid: ", criteria.ToString());

        return this;
    }
}