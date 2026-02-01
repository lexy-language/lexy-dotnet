using System;
using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser.Logging;

public static class NodesLogger
{
    public static string Log(IEnumerable<INode> nodes)
    {
        var builder = new StringBuilder();
        Log(null, nodes, builder, 0);
        return builder.ToString();
    }

    private static void Log(INode parent, IEnumerable<INode> nodes, StringBuilder builder, int indent)
    {
        var index = 0;
        foreach (var node in nodes)
        {
            if (node == null)
            {
                throw new InvalidOperationException($"Node {index++} of '{parent.GetType()}' is null.");
            }
            Log(node, builder, indent);
        }
    }

    private static void Log(INode node, StringBuilder builder, int indent)
    {
        var indentString = new string(' ', indent);

        if (node is INodeWithName componentNode)
        {
            builder.AppendLine($"{componentNode.Reference,30} {indentString}{componentNode.GetType().Name}: {componentNode.Name} ({componentNode.ToString()})");
        }
        else
        {
            builder.AppendLine($"{node.Reference,30} {indentString}{node.GetType().Name} ({node.ToString()})");
        }

        var children = node.GetChildren();

        Log(node, children, builder, indent + 2);
    }
}
