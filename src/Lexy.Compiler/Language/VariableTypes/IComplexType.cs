using System.Collections.Generic;

namespace Lexy.Compiler.Language.VariableTypes;

public interface IComplexType
{
    VariableType MemberType(string name, IComponentNodeList componentNodes);

    IEnumerable<IComplexTypeVariable> GetVariables();
    IEnumerable<IComplexTypeFunction> GetFunctions();

    IComplexTypeVariable GetVariable(string name);
    IComplexTypeFunction GetFunction(string name);
}
