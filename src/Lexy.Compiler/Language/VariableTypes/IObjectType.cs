using System.Collections.Generic;

namespace Lexy.Compiler.Language.VariableTypes;

public interface IObjectType
{
    VariableType MemberType(string name, IComponentNodeList componentNodes);

    IEnumerable<IObjectTypeVariable> GetVariables();
    IEnumerable<IObjectTypeFunction> GetFunctions();

    IObjectTypeVariable GetVariable(string name);
    IObjectTypeFunction GetFunction(string name);
}
