using System.Collections.Generic;

namespace Lexy.Compiler.Language;

internal interface IHasNodeDependencies
{
    IEnumerable<IRootNode> GetDependencies(RootNodeList rootNodeList);
}