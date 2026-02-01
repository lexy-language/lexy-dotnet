using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lexy.Tests;

public class VerifyCollectionContext<TItem> : VerifyModelContext<IReadOnlyList<TItem>>
    where TItem : class
{
    public VerifyCollectionContext(IReadOnlyList<TItem> model, VerifyLogging logging) : base(model, logging)
    {
    }

    public VerifyCollectionContext<TItem> Length(int length, string extraMessage)
    {
        var suffix = Normalize($" ({extraMessage})");
        Logging.LogAssert(Model.Count == length, nameof(Length), $"- Length Failed '{{0}}' != '{{1}}' {suffix}: ", Model.Count, length);
        return this;
    }

    public VerifyCollectionContext<TItem> ValueAt(int index, Func<TItem, bool> verify)
    {
        var value = index >= 0 && index < Model.Count ? Model[index] : null;
        if (value != null)
        {
            var valid = verify(value);
            Logging.LogAssert(valid, value.ToString(), "- ValueAt[{0}] not as expected: ", index);
            return this;
        }

        Logging.LogAssert(false, "null", "- ValueAtEquals[{0}] invalid: ", index);

        return this;
    }

    public VerifyCollectionContext<TItem> Contains(TItem expected)
    {
        var value = Model.Contains(expected);

        Logging.LogAssert(value, "collection", "- Contains[{0}] invalid: ", expected);

        return this;
    }

    public VerifyCollectionContext<TItem> Any(Expression<Func<TItem, bool>> criteria, string extraMessage)
    {
        var value = Model.Any(criteria.Compile());

        var suffix = Normalize($" - ({extraMessage})");
        Logging.LogAssert(value, criteria.ToString(), $"- Any invalid{suffix}: ");

        return this;
    }

    public VerifyCollectionContext<TItem> None(Expression<Func<TItem, bool>> criteria, string extraMessage)
    {
        var value = !Model.Any(criteria.Compile());

        var suffix = Normalize($" - ({extraMessage})");
        Logging.LogAssert(value, criteria.ToString(), $"- None invalid{suffix}: ");

        return this;
    }

    private static string Normalize(string value)
    {
        if (value == null) return string.Empty;
        return value.Replace("{", "{{").Replace("}", "}}");
    }
}
