using System;
using System.Collections.Generic;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Context;

public class ValidationContext : IValidationContext
{
    private readonly Stack<VariableContext> contexts = new();
    private VariableContext variableContext;

    public ILibraries Libraries { get; }
    public IParserLogger Logger { get; }
    public ISymbols Symbols { get; }
    public ComponentNodeList ComponentNodes { get; }

    public ITreeValidationVisitor Visitor { get; }

    public IVariableContext VariableContext
    {
        get
        {
            if (variableContext == null) throw new InvalidOperationException("FunctionCodeContext not set.");
            return variableContext;
        }
    }

    public ValidationContext(IParserLogger logger, ComponentNodeList componentNodes, ITreeValidationVisitor visitor, ILibraries libraries, ISymbols symbols)
    {
        Logger = Assert.NotNull(logger, nameof(logger));
        ComponentNodes = Assert.NotNull(componentNodes, nameof(componentNodes));
        Visitor = Assert.NotNull(visitor, nameof(visitor));
        Libraries = Assert.NotNull(libraries, nameof(libraries));
        Symbols = Assert.NotNull(symbols, nameof(symbols));
    }

    public void InNodeVariableScope(INode node, Action<IValidationContext> action)
    {
        StoreCurrentVariableContext();

        variableContext = new VariableContext(ComponentNodes, Logger, variableContext);

        action(this);

        var result = variableContext.ScopedVariables();
        Symbols.AddNodeVariables(node, result);

        RevertToPreviousVariableContext();
    }

    private void StoreCurrentVariableContext()
    {
        if (variableContext != null)
        {
            contexts.Push(variableContext);
        }
    }

    private void RevertToPreviousVariableContext()
    {
        variableContext = contexts.Count == 0 ? null : contexts.Pop();
    }
}
