using Lexy.Compiler.Language.Enums;

namespace Lexy.Compiler.Language.TypeSystem;

public class EnumMemberType : Type
{
    private readonly EnumDefinition enumDefinition;

    public EnumMemberType(EnumDefinition enumDefinition)
    {
        this.enumDefinition = enumDefinition;
    }

    public override bool IsAssignableFrom(Type type)
    {
        return type.Equals(enumDefinition.CreateType());
    }
}
