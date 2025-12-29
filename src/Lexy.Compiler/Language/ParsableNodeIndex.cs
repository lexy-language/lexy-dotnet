using System;

namespace Lexy.Compiler.Language;

public class ParsableNodeIndex
{
    private IParsableNode[] values = new IParsableNode[8];

    public ParsableNodeIndex(IParsableNode componentNode)
    {
        values[0] = componentNode;
    }

    public IParsableNode GetCurrentOrDescend(int indent)
    {
        var node = values[indent];
        ClearPreviousChildren(indent);
        return node;
    }

    private void ClearPreviousChildren(int indent)
    {
        var index = indent + 1;
        while (index < values.Length && values[indent++] != null)
        {
            values[index] = null;
        }
    }

    public void Set(int indent, IParsableNode node)
    {
        if (indent >= values.Length) Array.Resize(ref values, values.Length * 2);

        values[indent] = node;
    }

    public IComponentNode GetParentComponent(int indent)
    {
        while (indent >= 0)
        {
            if (values[indent--] is IComponentNode componentNode)
            {
                return componentNode;
            }
        }

        throw new InvalidOperationException("Can't find an IComponentNode parent.");
    }
}