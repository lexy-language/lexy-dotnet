using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public class DeclaredType : ObjectType
{
    public ITypeDefinition TypeDefinition { get; }

    public DeclaredType(ITypeDefinition typeDefinition) :
        base(typeDefinition.Name)
    {
        TypeDefinition = typeDefinition;
    }

    protected bool Equals(DeclaredType other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DeclaredType)obj);
    }

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return TypeDefinition;
    }

    protected override IEnumerable<IObjectMember> CreateMembers()
    {
        return TypeDefinition.Variables
            .Select(variable => new ObjectVariable(variable.Name, variable.TypeDeclaration.Type))
            .ToArray();
    }

    public override string ToString()
    {
        return Name;
    }

    public override Symbol GetSymbol(SourceReference reference)
    {
        return new Symbol(reference, $"type: {Name}", string.Empty, SymbolKind.Type);
    }
}
