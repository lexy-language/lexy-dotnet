using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class TrackLoggingCurrentNodeVisitor : ITreeValidationVisitor
{
    private readonly Stack<IComponentNode> nodeStack = new();
    private readonly IParserLogger logger;

    public TrackLoggingCurrentNodeVisitor(IParserLogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Enter(INode node)
    {
        if (node is not IComponentNode componentNode) return;

        AddCurrentNodeToStack(componentNode);

        logger.SetCurrentNode(componentNode);
    }

    public void Leave(INode node)
    {
        if (node is not IComponentNode) return;

        RemoveCurrentNodeFromStack();

        RevertToPreviousNode();
    }

    private void AddCurrentNodeToStack(IComponentNode componentNode) => nodeStack.Push(componentNode);
    private void RemoveCurrentNodeFromStack() => nodeStack.Pop();

    private void RevertToPreviousNode()
    {
        if (nodeStack.TryPeek(out var previousNode))
        {
            logger.SetCurrentNode(previousNode);
        }
    }
}