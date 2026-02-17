using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language;

public interface IParsableNode : INode
{
    IParsableNode Parse(IParseLineContext context);
}
