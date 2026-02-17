using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Parser;

public class ParserResult
{
    public LexyScriptNode RootNode { get; }
    public ComponentNodeList Nodes { get; }
    public IParserLogger Logger { get; }
    public Dependencies Dependencies { get; }
    public ISymbols Symbols { get; }

    public ParserResult(LexyScriptNode rootNode, ComponentNodeList nodes, IParserLogger logger, Dependencies dependencies, ISymbols symbols)
    {
        RootNode = rootNode;
        Nodes = nodes;
        Logger = logger;
        Dependencies = dependencies;
        Symbols = symbols;
    }
}
