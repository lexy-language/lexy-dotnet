using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem;

public class EnumMemberType : Type
{
    private readonly EnumDefinition enumDefinition;
    private readonly string name;
    public EnumMemberType(EnumDefinition enumDefinition, string name)
    {
        this.name = name;
        this.enumDefinition = enumDefinition;
    }

    public override bool IsAssignableFrom(Type type)
    {
        return type.Equals(enumDefinition.CreateType());
    }

    public override string ToString()
    {
        return $"{enumDefinition.Name}.{name}";
    }

    public override Symbol GetSymbol(SourceReference reference)
    {
        return new Symbol(reference, $"enum member: {enumDefinition.Name}.{name}", string.Empty, SymbolKind.EnumMember);
    }
}
