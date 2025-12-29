using System.Collections.Generic;

namespace Lexy.Compiler.Language;

internal interface IHasNodeDependencies
{
    IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes);
}