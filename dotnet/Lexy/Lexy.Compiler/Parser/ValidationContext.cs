using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser
{
    public class ValidationContext : IValidationContext
    {
        private class CodeContextScope : IDisposable
        {
            private readonly Func<IVariableContext> func;

            public CodeContextScope(Func<IVariableContext> func) => this.func = func;

            public void Dispose() => func();
        }

        private readonly Stack<IVariableContext> contexts = new Stack<IVariableContext>();
        private IVariableContext variableContext;

        public IParserContext ParserContext { get; }

        public IVariableContext VariableContext
        {
            get
            {
                if (variableContext == null)
                {
                    throw new InvalidOperationException("FunctionCodeContext not set.");
                }
                return variableContext;
            }
        }

        public IParserLogger Logger => ParserContext.Logger;
        public RootNodeList RootNodes => ParserContext.Nodes;

        public ValidationContext(IParserContext context)
        {
            ParserContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IDisposable CreateVariableScope()
        {
            if (variableContext != null)
            {
                contexts.Push(variableContext);
            }

            variableContext = new VariableContext(Logger, variableContext);

            return new CodeContextScope(() =>
            {
                return variableContext = contexts.Count == 0 ? null : contexts.Pop();
            });
        }
    }
}