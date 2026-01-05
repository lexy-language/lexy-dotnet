using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserResult
{
    public LexyScriptNode RootNode { get; }
    public ComponentNodeList Nodes { get; }
    public IParserLogger Logger { get; }

    public ParserResult(LexyScriptNode rootNode, ComponentNodeList nodes, IParserLogger logger)
    {
        RootNode = rootNode;
        Nodes = nodes;
        Logger = logger;
    }
}