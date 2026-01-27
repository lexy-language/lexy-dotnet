using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser.Symbols;

public class DocumentSymbols
{
    private record ReturnValue(Symbol Symbol);

    private readonly List<INode> nodes = new();

    public SymbolDescription GetDescription(Position position) => MapDescription(GetNode(position));

    public Signatures GetSignatures(Position position) => MapSignatures(GetNode(position));

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

    public void Add(IParsableNode parsedNode)
    {
        nodes.Add(parsedNode);
    }
}
