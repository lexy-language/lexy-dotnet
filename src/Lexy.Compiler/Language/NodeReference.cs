using System;

namespace Lexy.Compiler.Language;

public class NodeReference
{
    private bool isSet;
    private INode node;

    public INode Node
    {
        get
        {
            if (!isSet) throw new InvalidOperationException("Node object reference not set.");
            return node;
        }
    }

    public NodeReference()
    {
    }

    public NodeReference(INode node)
    {
        this.node = node;
        isSet = true;
    }

    public void SetNode(INode node)
    {
        if (isSet) throw new InvalidOperationException("NodeObjectReference can't be set twice.");
        this.node = node;
        isSet = true;
    }
}
