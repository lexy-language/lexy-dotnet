using System.Collections.Generic;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public interface IObjectType
{
    string Name { get; }

    Type MemberType(string name);

    bool ContainsMember(string name);
    IObjectMember GetMember(string name);

    IEnumerable<ObjectVariable> GetVariables();
    IEnumerable<ObjectFunction> GetFunctions();

    ObjectVariable GetVariable(string name);
    ObjectFunction GetFunction(string name);

    IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes);
}
