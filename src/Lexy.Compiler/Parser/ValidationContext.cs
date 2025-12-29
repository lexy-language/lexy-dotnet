using System;
using System.Collections.Generic;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ValidationContext : IValidationContext
{
    private class OnDispose : IDisposable
    {
        private readonly Action onDispose;

        public OnDispose(Action onDispose) => this.onDispose = onDispose;

        public void Dispose() => onDispose();
    }

    private readonly Stack<IVariableContext> contexts = new();
    private IVariableContext variableContext;

    public ILibraries Libraries { get; }
    public IParserLogger Logger { get; }
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

    public ValidationContext(IParserLogger logger, ComponentNodeList componentNodes, ITreeValidationVisitor visitor, ILibraries libraries)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ComponentNodes = componentNodes ?? throw new ArgumentNullException(nameof(componentNodes));
        Visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
        Libraries = libraries ?? throw new ArgumentNullException(nameof(libraries));
    }

    public IDisposable CreateVariableScope()
    {
        StoreCurrentVariableContext();

        variableContext = new VariableContext(ComponentNodes, Logger, variableContext);

        return new OnDispose(RevertToPreviousVariableContext);
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