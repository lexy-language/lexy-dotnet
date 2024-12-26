using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public interface INode
    {
        SourceReference Reference { get; }

        void ValidateTree(IValidationContext context);

        IEnumerable<INode> GetChildren();
    }
}