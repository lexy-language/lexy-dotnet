using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace Lexy.Tests;

public class Context<TModel>
{
    private readonly TModel model;
    private readonly VerifyLogging logging;

    public Context(TModel model, VerifyLogging logging)
    {
        this.model = model;
        this.logging = logging;
    }

    public Context<TModel> Fail(string format, params object[] args)
    {
        var message = string.Format(CultureInfo.InvariantCulture, format, args);
        logging.AppendLine(">> " + message);
        logging.ErrorOccured();
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, bool>> expression, bool actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual<T>(Expression<Func<TModel, T>> expression, T actual) where T : IComparable
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value.CompareTo(actual) == 0, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual<T>(Expression<Func<TModel, T?>> expression, T? actual) where T : struct, IComparable
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(
            (value == null && actual == null)
            || (value != null && actual != null && value.Value.CompareTo(actual.Value) == 0), message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, bool?>> expression, bool? actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, Guid>> expression, Guid actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, string>> expression, string actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, int>> expression, int actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, int?>> expression, int? actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, decimal>> expression, decimal actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, DateTime>> expression, DateTime actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreEqual(Expression<Func<TModel, DateTime?>> expression, DateTime? actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == actual, message, "- AreEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreNotEqual(Expression<Func<TModel, string>> expression, string actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value != actual, message, "- AreNotEqual Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> AreSame<T>(Expression<Func<TModel, T>> expression, T actual)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(ReferenceEquals(value, actual), message, "- AreSame Failed '{0}' != '{1}': ", value, actual);
        return this;
    }

    public Context<TModel> IsEmpty(Expression<Func<TModel, string>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == string.Empty, message, "- IsEmpty Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsNotNull(Expression<Func<TModel, object>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value != null, message, "- IsNotNull Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsNotNull<TSubModel>(Expression<Func<TModel, TSubModel>> expression, Action<Context<TSubModel>> subContext)
    {
        var (value, message) = expression.CompileExpression(model);
        var valid = value != null;
        if (valid) {
            return InContext(subContext, value);
        }
        logging.LogAssert(false, message, "- IsNotNull Failed '': ");
        return this;
    }

    public Context<TModel> IsNull(Expression<Func<TModel, object>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == null, message, "- IsNull Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsTrue(Expression<Func<TModel, bool>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value, message, "- IsTrue Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsTrue(Expression<Func<TModel, bool?>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == true, message, "- IsTrue Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsFalse(Expression<Func<TModel, bool>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(!value, message, "- IsFalse Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> IsFalse(Expression<Func<TModel, bool?>> expression)
    {
        var (value, message) = expression.CompileExpression(model);
        logging.LogAssert(value == false, message, "- IsFalse Failed '{0}': ", value);
        return this;
    }

    public Context<TModel> AssertThrowsException<TException>(Action assertAction, string message = null) where TException : Exception
    {
        try
        {
            assertAction();
            logging.LogAssert(false, message, "- AssertThrowsException Failed: No Exception thorwn");
            return this;
        }
        catch (Exception exception)
        {
            logging.LogAssert(exception.GetType() == typeof(TException), message, "- AssertThrowsException Failed: Wrong exception type: '{0}'", exception.GetType());

            return this;
        }
    }

    public Context<TModel> IsOfType<TExpected>(Expression<Func<TModel, object>> expression, Action<Context<TExpected>> subContext) where TExpected : class
    {
        var (value, message) = expression.CompileExpression(model);
        var subInstance = value as TExpected;
        var valid = subInstance != null;
        if (valid)
        {
            return InContext(subContext, subInstance);
        }

        logging.LogAssert(false, message, "- IsOfType<{0}> Failed '{1}': ", typeof(TExpected), value != null ? value.GetType().ToString() : "<null>");
        return this;
    }

    public Context<TModel> CountIs<T>(Expression<Func<TModel, IReadOnlyList<T>>> collection, int expected)
    {
        var (value, message) = collection.CompileExpression(model);
        var valid = value.Count == expected;

        logging.LogAssert(valid, message, "- CountIs Failed '{0}' != '{1}': ", value.Count, expected);
        return this;
    }

    public Context<TModel> CountIs<T>(Expression<Func<TModel, IReadOnlyCollection<T>>> collection, int expected)
    {
        var (value, message) = collection.CompileExpression(model);
        var valid = value.Count == expected;

        logging.LogAssert(valid, message, "- CountIs Failed '{0}' != '{1}': ", value.Count, expected);
        return this;
    }

    public Context<TModel> ContainsKey<TKey, TValue>(Expression<Func<TModel, IDictionary<TKey, TValue>>> collection, TKey key)
    {
        var (collectionValue, message) = collection.CompileExpression(model);
        var valid = collectionValue.TryGetValue(key, out var value);

        logging.LogAssert(valid, message, "- ContainsKey Failed '{0}': ", key);
        return this;
    }

    public Context<TModel> ContainsKey<TKey, TValue>(Expression<Func<TModel, IDictionary<TKey, TValue>>> collection, TKey key, Action<Context<TValue>> subContext)
    {
        var (collectionValue, message) = collection.CompileExpression(model);
        var valid = collectionValue.TryGetValue(key, out var value);
        if (valid)
        {
            return InContext(subContext, value);
        }

        logging.LogAssert(valid, message, "- ContainsKey '{0}': ", key);
        return this;
    }

    public Context<TModel> ValueAt<TItem>(Expression<Func<TModel, IReadOnlyList<TItem>>> list, int index, Action<Context<TItem>> subContext)
        where TItem : class
    {
        var (listValue, message) = list.CompileExpression(model);
        var value = index >= 0 && index < listValue.Count ? listValue[index] : null;
        if (value != null)
        {
            return InContext(subContext, value);
        }

        logging.LogAssert(false, message, "- ValueAt '{0}': ", index);
        return this;
    }

    public Context<TModel> ValueAtEquals<TItem>(Expression<Func<TModel, IReadOnlyList<TItem>>> list, int index, TItem expected)
        where TItem : struct, IComparable
    {
        var (listValue, message) = list.CompileExpression(model);
        TItem? value = index >= 0 && index < listValue.Count ? listValue[index] : null;
        if (value != null)
        {
            logging.LogAssert(expected.CompareTo(value.Value) == 0, message, "- ValueAtEquals[{0}] '{1}' != '{2}': ", index, expected, value);
            return this;
        }

        logging.LogAssert(false, message, "- ValueAtEquals[{0}] invalid: ", index);

        return this;
    }

    public Context<TModel> ValueAtEquals<TItem, TValue>(Expression<Func<TModel, IReadOnlyList<TItem>>> list, int index, Expression<Func<TItem, TValue>> property, TValue expected)
        where TItem : class
        where TValue : IComparable
    {
        var (listValue, itemMessage) = list.CompileExpression(model);
        TItem item = index >= 0 && index < listValue.Count ? listValue[index] : null;
        if (item != null)
        {
            var (propertyValue, propertyMessage) = property.CompileExpression(item);

            logging.LogAssert(expected.CompareTo(propertyValue) == 0, propertyMessage, "- ValueAtEquals[{0}] '{1}' != '{2}': ", index, expected, propertyValue);
            return this;
        }

        logging.LogAssert(false, itemMessage, "- ValueAtEquals[{0}] invalid: ", index);

        return this;
    }

    private Context<TModel> InContext(Action<Context<TModel>> subContext)
    {
        logging.WithIndentation(() => subContext(this));

        return this;
    }

    private Context<TModel> InContext<TSubModel>(Action<Context<TSubModel>> subContext, TSubModel value)
    {
        logging.WithIndentation(() => subContext(new Context<TSubModel>(value, logging)));

        return this;
    }
}