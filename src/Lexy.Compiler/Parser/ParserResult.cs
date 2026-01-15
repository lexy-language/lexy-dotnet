using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserResult
{
    public LexyScriptNode RootNode { get; }
    public ComponentNodeList Nodes { get; }
    public IParserLogger Logger { get; }
    public Dependencies Dependencies { get; }

    public ParserResult(LexyScriptNode rootNode, ComponentNodeList nodes, IParserLogger logger, Dependencies dependencies)
    {
        RootNode = rootNode;
        Nodes = nodes;
        Logger = logger;
        Dependencies = dependencies;
    }
}
