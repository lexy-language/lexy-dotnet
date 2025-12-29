using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class TypeWithMembers : VariableType, ITypeWithMembers
{
    public abstract VariableType MemberType(string name, IComponentNodeList componentNodes);

    public virtual IInstanceFunction GetFunction(string name) => null;
}