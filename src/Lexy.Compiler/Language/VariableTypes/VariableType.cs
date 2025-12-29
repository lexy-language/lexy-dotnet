using System.Collections.Generic;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class VariableType : IHasNodeDependencies
{
    public virtual IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield break;
    }
}