using System;

namespace Lexy.RunTime;

public static class Assert
{
    public static void True(bool condition, string error)
    {
        if (!condition)
        {
            throw new InvalidOperationException(error);
        }
    }

    public static void False(bool condition, string error)
    {
        if (condition)
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

    public static T NotNull<T>(T value, string name)
    {
        if (value == null)
        {
            throw new InvalidOperationException($"'{name}' should not be null.");
        }

        return value;
    }

    public static void Fail(string error)
    {
        throw new InvalidOperationException(error);
    }

    public static void NotNullOrEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"'{name}' should not be null or empty.");
        }
    }
}
