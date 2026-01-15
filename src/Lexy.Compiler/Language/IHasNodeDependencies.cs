using System.Collections.Generic;

namespace Lexy.Compiler.Language;

internal interface IHasNodeDependencies : INode
{
    IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes);
}
