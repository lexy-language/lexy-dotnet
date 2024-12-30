using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public interface IParsableNode : INode
{
    IParsableNode Parse(IParseLineContext context);
}
