using System.Collections.Generic;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class VariableType
{
    public virtual IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodeList)
    {
        yield break;
    }
}