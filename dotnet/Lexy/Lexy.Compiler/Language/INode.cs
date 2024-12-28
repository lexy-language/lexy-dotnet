using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public interface INode
{
    SourceReference Reference { get; }

    void ValidateTree(IValidationContext context);

    IEnumerable<INode> GetChildren();
}