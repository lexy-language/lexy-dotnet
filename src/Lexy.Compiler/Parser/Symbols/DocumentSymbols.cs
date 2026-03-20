using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Symbols;

public class DocumentSymbols : IDocumentSymbols
{
    private record ReturnValue(Symbol Symbol);

    private readonly INode lexyScriptNode;
    private readonly List<INode> nodes = new();
    private Line[] lines = new Line[32];

    public DocumentSymbols(INode lexyScriptNode)
    {
        this.lexyScriptNode = Assert.NotNull(lexyScriptNode, nameof(lexyScriptNode));
    }

    public SymbolDescription GetDescription(Position position) => MapDescription(GetNode(position));

    public Signatures GetSignatures(Position position) => MapSignatures(GetNode(position));

    public void Add(IComponentNode parsedNode)
    {
        nodes.Add(parsedNode);
    }

    public void WalkSymbols(Action<INode, Symbol> symbolWalker)
    {
        NodesWalker.Walk(nodes, node =>
        {
            var symbol = node.GetSymbol();
            if (symbol != null)
            {
                symbolWalker(node, symbol);
            }
        });
    }

    public void Add(Line line)
    {
        if (line.Index >= lines.Length)
        {
            Array.Resize(ref lines, lines.Length + 32);
        }

        Assert.True(line.Index < lines.Length, "Lines should be added sequentially");

        lines[line.Index] = line;
    }

    private SymbolDescription MapDescription(Symbol symbol)
    {
        if (symbol == null) return null;
        return new SymbolDescription(symbol.Name, symbol.Description, symbol.Kind);
    }

    private Signatures MapSignatures(Symbol symbol)
    {
        if (symbol == null) return null;
        throw new NotImplementedException();
    }

    private Symbol GetNode(Position position)
    {
        Symbol previous = null;
        var symbol = GetSymbol(position, nodes, ref previous);
        Console.WriteLine($">>>>>: ({position}) - `{symbol}`");
        return symbol?.Symbol ?? previous;
    }

    private static ReturnValue GetSymbol(Position position, IReadOnlyList<INode> list, ref Symbol previousSymbol)
    {
        foreach (var node in list)
        {
            Console.WriteLine($"Check: ({position}) between '{previousSymbol?.Reference}' and '{node.Reference}'");

            if (node.Reference.LineNumber > position.LineNumber)
            {
                return new ReturnValue(previousSymbol);
            }

            if (node.Reference.LineNumber == position.LineNumber && node.Reference.Column > position.Column)
            {
                return new ReturnValue(previousSymbol);
            }

            if (node.Reference.Includes(position))
            {
                var symbol = node.GetSymbol();
                if (symbol != null)
                {
                    previousSymbol = symbol;
                }
            }

            var childSymbol = GetSymbol(position, node.GetChildren().ToList(), ref previousSymbol);
            if (childSymbol != null) return childSymbol;
        }

        return null;
    }

    public IReadOnlyList<INode> GetNodesInScope(Position position)
    {
        var nodesInScope = new List<List<INode>>();
        GetNodesInScope(position, nodes, nodesInScope);

        return nodesInScope.Count == 0
            ? new List<INode>{lexyScriptNode}
            : Flatten(nodesInScope);
    }

    private static List<INode> Flatten(List<List<INode>> nodesInScope)
    {
        var result = new List<INode>();
        foreach (var nodes in nodesInScope)
        {
            result.AddRange(nodes);
        }
        return result;
    }

    private static void GetNodesInScope(Position position, IEnumerable<INode> list, ICollection<List<INode>> nodesInScope)
    {
        var wasIn = false;
        var precedingNodes = new List<INode>();
        foreach (var node in list)
        {
            var inNode = node.Area.Includes(position);

            if (wasIn && !inNode) return;

            if (nodesInScope.Count > 0)
            {
                precedingNodes.Add(node);
            }

            if (inNode)
            {
                if (nodesInScope.Count == 0)
                {
                    precedingNodes.Add(node);
                }

                nodesInScope.Add(precedingNodes);
                GetNodesInScope(position, node.GetChildren(), nodesInScope);
                wasIn = true;
            }
        }
    }

    public Token GetToken(Position position)
    {
        Assert.NotNull(position, nameof(position));
        Assert.True(lines.Length >= position.LineNumber, $"Couldn't find line: {position.LineNumber} Lines: {lines.Length}");

        var line = lines[position.LineNumber - 1];
        Assert.NotNull(line, $"Couldn't find line: {position.LineNumber} Lines: {lines.Length}");

        return line.Tokens.TokenAt(position.Column);
    }
}
