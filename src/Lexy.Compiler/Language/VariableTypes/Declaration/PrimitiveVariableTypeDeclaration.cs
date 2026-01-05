using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Declaration;

public sealed class PrimitiveVariableTypeDeclaration : VariableTypeDeclaration
{
    public string Type { get; }

    public PrimitiveVariableTypeDeclaration(string type, SourceReference reference) : base(reference)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    protected bool Equals(PrimitiveVariableTypeDeclaration other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PrimitiveVariableTypeDeclaration)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    protected override VariableType ValidateVariableType(IValidationContext context)
    {
        return new PrimitiveType(Type);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }
}