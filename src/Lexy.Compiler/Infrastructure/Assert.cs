using System;

namespace Lexy.Compiler.Infrastructure;

public static class Assert
{
    public static void True(bool condition, string error)
    {
        if (!condition)
        {
            throw new InvalidOperationException(error);
        }
    }

    public static T Is<T>(object value, string name)
    {
        NotNull(value, name);
        if (value is T specific)
        {
            return specific;
        }
        throw new InvalidOperationException($"'{name}' should be of type '{typeof(T).Name}', but is '{value.GetType().Name}'");
    }

    public static void NotNull(object value, string name)
    {
        if (value == null)
        {
            throw new InvalidOperationException($"'{name}' should not be null.");
        }
    }

    public static void Fail(string error)
    {
        throw new InvalidOperationException(error);
    }
}