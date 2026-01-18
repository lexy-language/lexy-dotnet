using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public sealed class PrimitiveTypeDeclaration : TypeDeclaration
{
    public string TypeName { get; }

    public PrimitiveTypeDeclaration(string type, SourceReference reference) : base(reference)
    {
        TypeName = Assert.NotNull(type, nameof(type));
    }

    protected bool Equals(PrimitiveTypeDeclaration other)
    {
        return TypeName == other.TypeName;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PrimitiveTypeDeclaration)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return TypeName;
    }

    protected override Type ValidateVariableType(IValidationContext context)
    {
        return new ValueType(TypeName);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }
}
