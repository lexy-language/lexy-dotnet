using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser.Logging;

public static class NodesLogger
{
    public static void Log(IEnumerable<INode> nodes, Action<string> logger)
    {
        Log(null, nodes, logger, 0);
    }

    private static void Log(INode parent, IEnumerable<INode> nodes, Action<string> logger, int indent)
    {
        var index = 0;
        foreach (var node in nodes)
        {
            if (node == null)
            {
                throw new InvalidOperationException($"Node {index++} of '{parent.GetType()}' is null.");
            }
            Log(node, logger, indent);
        }
    }

    private static void Log(INode node, Action<string> logger, int indent)
    {
        var indentString = new string(' ', indent);

        logger($"{node.Reference,-70} {indentString}{node.GetType().Name}: {node.ToString()}");

        var children = node.GetChildren();

        Log(node, children, logger, indent + 2);
    }
}
