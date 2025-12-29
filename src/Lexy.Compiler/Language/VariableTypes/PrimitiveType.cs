using System;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Language.VariableTypes;

public class PrimitiveType : VariableType
{
    public static PrimitiveType Boolean => new(TypeNames.Boolean);
    public static PrimitiveType String => new(TypeNames.String);
    public static PrimitiveType Number => new(TypeNames.Number);
    public static PrimitiveType Date => new(TypeNames.Date);

    public string Type { get; }

    public PrimitiveType(string type)
    {
        Type = type;
    }

    protected bool Equals(PrimitiveType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PrimitiveType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    public static VariableType Parse(Type type)
    {
        if (type == typeof(bool)) return Boolean;
        if (type == typeof(string)) return String;
        if (type == typeof(int)) return Number;
        if (type == typeof(double)) return Number;
        if (type == typeof(decimal)) return Number;
        if (type == typeof(DateTime)) return Date;
        throw new InvalidOperationException($"Invalid primitive type: '{type.Namespace}.{type.Name}'");
    }
}