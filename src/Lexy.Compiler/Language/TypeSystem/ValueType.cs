using System;

namespace Lexy.Compiler.Language.TypeSystem;

public class ValueType : Type
{
    public static ValueType Boolean => new(TypeNames.Boolean);
    public static ValueType String => new(TypeNames.String);
    public static ValueType Number => new(TypeNames.Number);
    public static ValueType Date => new(TypeNames.Date);

    public string Type { get; }

    public ValueType(string type)
    {
        Type = type;
    }

    public override bool IsAssignableFrom(Type type) => Equals(type);

    protected bool Equals(ValueType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ValueType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    public static Type Parse(System.Type type)
    {
        if (type == typeof(bool)) return Boolean;
        if (type == typeof(string)) return String;
        if (type == typeof(int)) return Number;
        if (type == typeof(double)) return Number;
        if (type == typeof(decimal)) return Number;
        if (type == typeof(DateTime)) return Date;
        throw new InvalidOperationException($"Invalid value type: '{type.Namespace}.{type.Name}'");
    }
}
