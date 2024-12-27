using System.Collections.Generic;

namespace Lexy.Compiler.Language.Expressions.Functions
{
    internal interface IHasNodeDependencies
    {
        IEnumerable<IRootNode> GetDependencies(Nodes nodes);
    }
}