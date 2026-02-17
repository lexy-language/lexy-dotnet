using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Lexy.RunTime;

namespace Lexy.Tests;

public class VerifyModelContext<TModel>
{
    protected TModel Model { get; }
    protected VerifyLogging Logging { get; }

    public VerifyModelContext(TModel model, VerifyLogging logging)
    {
        Model = Assert.NotNull(model, nameof(model));
        Logging = Assert.NotNull(logging, nameof(logging));
    }

    public VerifyModelContext<TModel> Collection<TItem>(Expression<Func<TModel, IReadOnlyList<TItem>>> expression, Action<VerifyCollectionContext<TItem>> handler) where TItem : class
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.AppendLine("Collection: " + message);
        handler(new VerifyCollectionContext<TItem>(value, Logging));
        return this;
    }

    public VerifyModelContext<TModel> Fail(string format, params object[] args)
    {
        var message = string.Format(CultureInfo.InvariantCulture, format, args);
        Logging.AppendLine(">> " + message);
        Logging.ErrorOccurred();
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, bool>> expression, bool actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual<T>(Expression<Func<TModel, T>> expression, T actual, string extraMessage = null) where T : IComparable
    {
        var (value, message) = expression.CompileExpression(Model);
        var suffix = extraMessage != null ? $" ({extraMessage})" : "";
        Logging.LogAssert(value.CompareTo(actual) == 0, message, $"- AreEqual Failed '{{0}}' != '{{1}}'{suffix}: ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual<T>(Expression<Func<TModel, T?>> expression, T? actual) where T : struct, IComparable
    {
        var (value, message) = expression.CompileExpression(Model);
        var valid = value == null && actual == null
                    || (value != null && actual != null && value.Value.CompareTo(actual.Value) == 0);
        Logging.LogAssert(valid, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, bool?>> expression, bool? actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, Guid>> expression, Guid actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, string>> expression, string actual, string extraMessage = null)
    {
        var (value, message) = expression.CompileExpression(Model);
        var suffix = extraMessage != null ? $"({extraMessage})" : "";
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}'" + suffix + ": ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, int>> expression, int actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, int?>> expression, int? actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, decimal>> expression, decimal actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, DateTime>> expression, DateTime actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreEqual(Expression<Func<TModel, DateTime?>> expression, DateTime? actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreNotEqual(Expression<Func<TModel, string>> expression, string actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value != actual, message, "- AreNotEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> AreSame<T>(Expression<Func<TModel, T>> expression, T actual)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(ReferenceEquals(value, actual), message, "- AreSame Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public VerifyModelContext<TModel> IsEmpty(Expression<Func<TModel, string>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == string.Empty, message, "- IsEmpty Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsNotNull(Expression<Func<TModel, object>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value != null, message, "- IsNotNull Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsNotNull<TSubModel>(Expression<Func<TModel, TSubModel>> expression, Action<VerifyModelContext<TSubModel>> subContext, string extraMessage = null)
    {
        var (value, message) = expression.CompileExpression(Model);
        var valid = value != null;
        if (valid)
        {
            return InContext(subContext, value);
        }
        Logging.LogAssert(false, message, $"- IsNotNull Failed '{extraMessage}': ");
        return this;
    }

    public VerifyModelContext<TModel> IsNull(Expression<Func<TModel, object>> expression, string extraMessage = null)
    {
        var (value, message) = expression.CompileExpression(Model);
        var suffix = extraMessage != null ? $" ({extraMessage})" : "";
        Logging.LogAssert(value == null, message, $"- IsNull Failed '{{0}}'{suffix}: ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsTrue(Expression<Func<TModel, bool>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value, message, "- IsTrue Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsTrue(Expression<Func<TModel, bool?>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == true, message, "- IsTrue Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsFalse(Expression<Func<TModel, bool>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(!value, message, "- IsFalse Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> IsFalse(Expression<Func<TModel, bool?>> expression)
    {
        var (value, message) = expression.CompileExpression(Model);
        Logging.LogAssert(value == false, message, "- IsFalse Failed '{0}': ", value);
        return this;
    }

    public VerifyModelContext<TModel> AssertThrowsException<TException>(Action assertAction, string message = null) where TException : Exception
    {
        try
        {
            assertAction();
            Logging.LogAssert(false, message, "- AssertThrowsException Failed: No Exception thorwn");
            return this;
        }
        catch (Exception exception)
        {
            Logging.LogAssert(exception.GetType() == typeof(TException), message, "- AssertThrowsException Failed: Wrong exception type: '{0}'", exception.GetType());
            return this;
        }
    }

    public VerifyModelContext<TModel> IsOfType<TExpected>(Expression<Func<TModel, object>> expression, Action<VerifyModelContext<TExpected>> subContext = null) where TExpected : class
    {
        var (value, message) = expression.CompileExpression(Model);
        var subInstance = value as TExpected;
        var valid = subInstance != null;
        if (valid)
        {
            return InContext(subContext, subInstance);
        }

        Logging.LogAssert(false, message, "- IsOfType<{0}> Failed '{1}': ", typeof(TExpected), value != null ? value.GetType().ToString() : "<null>");
        return this;
    }

    public VerifyModelContext<TModel> CountIs<T>(Expression<Func<TModel, IReadOnlyList<T>>> collection, int expected)
    {
        var (value, message) = collection.CompileExpression(Model);
        var valid = value.Count == expected;

        Logging.LogAssert(valid, message, "- CountIs Failed '{0}' != '{1}': ", value.Count, expected);
        return this;
    }

    public VerifyModelContext<TModel> CountIs<T>(Expression<Func<TModel, IReadOnlyCollection<T>>> collection, int expected)
    {
        var (value, message) = collection.CompileExpression(Model);
        var valid = value.Count == expected;

        Logging.LogAssert(valid, message, "- CountIs Failed '{0}' != '{1}': ", value.Count, expected);
        return this;
    }

    public VerifyModelContext<TModel> ContainsKey<TKey, TValue>(Expression<Func<TModel, IDictionary<TKey, TValue>>> collection, TKey key)
    {
        var (collectionValue, message) = collection.CompileExpression(Model);
        var valid = collectionValue.TryGetValue(key, out var value);

        Logging.LogAssert(valid, message, "- ContainsKey Failed '{0}': ", key);
        return this;
    }

    public VerifyModelContext<TModel> ContainsKey<TKey, TValue>(Expression<Func<TModel, IDictionary<TKey, TValue>>> collection, TKey key, Action<VerifyModelContext<TValue>> subContext)
    {
        var (collectionValue, message) = collection.CompileExpression(Model);
        var valid = collectionValue.TryGetValue(key, out var value);
        if (valid)
        {
            return InContext(subContext, value);
        }

        Logging.LogAssert(valid, message, "- ContainsKey '{0}': ", key);
        return this;
    }

    public VerifyModelContext<TModel> ValueAtEquals<TItem, TValue>(Expression<Func<TModel, IReadOnlyList<TItem>>> list, int index, Expression<Func<TItem, TValue>> property, TValue expected)
        where TItem : class
        where TValue : IComparable
    {
        var (listValue, itemMessage) = list.CompileExpression(Model);
        TItem item = index >= 0 && index < listValue.Count ? listValue[index] : null;
        if (item != null)
        {
            var (propertyValue, propertyMessage) = property.CompileExpression(item);

            Logging.LogAssert(expected.CompareTo(propertyValue) == 0, propertyMessage, "- ValueAtEquals[{0}] '{1}' != '{2}': ", index, expected, propertyValue);
            return this;
        }

        Logging.LogAssert(false, itemMessage, "- ValueAtEquals[{0}] invalid: ", index);

        return this;
    }

    public VerifyModelContext<TModel> IfNotNull<T>(T value, Action<VerifyModelContext<TModel>> subContext)
    {
        if (value != null)
        {
            subContext(this);
        }
        return this;
    }

    public void ForEach<TItem>(IEnumerable<TItem> items, Action<TItem> handler)
    {
        foreach (var item in items)
        {
            handler(item);
        }
    }

    private VerifyModelContext<TModel> InContext(Action<VerifyModelContext<TModel>> subContext)
    {
        Logging.WithIndentation(() => subContext(this));

        return this;
    }

    private VerifyModelContext<TModel> InContext<TSubModel>(Action<VerifyModelContext<TSubModel>> subContext,
        TSubModel value)
    {
        if (subContext != null)
        {
            Logging.WithIndentation(() => subContext(new VerifyModelContext<TSubModel>(value, Logging)));
        }
        return this;
    }
}
