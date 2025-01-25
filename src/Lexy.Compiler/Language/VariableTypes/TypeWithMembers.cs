using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class TypeWithMembers : VariableType, ITypeWithMembers
{
    public abstract VariableType MemberType(string name, IValidationContext context);
}