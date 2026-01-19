using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public sealed class ValueTypeDeclaration : TypeDeclaration
{
    public string TypeName { get; }

    public ValueTypeDeclaration(string type, SourceReference reference) : base(reference)
    {
        TypeName = Assert.NotNull(type, nameof(type));
    }

    protected bool Equals(ValueTypeDeclaration other)
    {
        return TypeName == other.TypeName;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ValueTypeDeclaration)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return TypeName;
    }

    protected override Type ValidateType(IValidationContext context)
    {
        return new ValueType(TypeName);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }
}
