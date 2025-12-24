using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserResult
{
    public ComponentNodeList Nodes { get; }
    public IParserLogger Logger { get; }

    public ParserResult(ComponentNodeList nodes, IParserLogger logger)
    {
        Nodes = nodes;
        Logger = logger;
    }
}