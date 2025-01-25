using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public interface ITypeWithMembers
{
    VariableType MemberType(string name, IValidationContext context);
}