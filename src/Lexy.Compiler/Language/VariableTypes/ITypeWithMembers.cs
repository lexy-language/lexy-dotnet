using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Language.VariableTypes;

public interface ITypeWithMembers
{
    VariableType MemberType(string name, IComponentNodeList componentNodes);

    IInstanceFunction GetFunction(string name);
}
