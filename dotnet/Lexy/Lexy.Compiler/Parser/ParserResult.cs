using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserResult
{
    public RootNodeList Nodes { get; }
    public IParserLogger Logger { get; }

    public ParserResult(RootNodeList nodes, IParserLogger logger)
    {
        Nodes = nodes;
        Logger = logger;
    }
}