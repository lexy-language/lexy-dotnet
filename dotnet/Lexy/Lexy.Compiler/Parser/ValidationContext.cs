using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser
{
    public class ValidationContext : IValidationContext
    {
        private class CodeContextScope : IDisposable
        {
            private readonly Func<IFunctionCodeContext> func;

            public CodeContextScope(Func<IFunctionCodeContext> func) => this.func = func;

            public void Dispose() => func();
        }

        private readonly Stack<IFunctionCodeContext> contexts = new Stack<IFunctionCodeContext>();
        private IFunctionCodeContext functionCodeContext;

        public IParserContext ParserContext { get; }

        public IFunctionCodeContext FunctionCodeContext
        {
            get
            {
                if (functionCodeContext == null)
                {
                    throw new InvalidOperationException("FunctionCodeContext not set.");
                }
                return functionCodeContext;
            }
        }

        public IParserLogger Logger => ParserContext.Logger;
        public Nodes Nodes => ParserContext.Nodes;

        public ValidationContext(IParserContext context)
        {
            ParserContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IDisposable CreateCodeContextScope()
        {
            if (functionCodeContext != null)
            {
                contexts.Push(functionCodeContext);
            }

            functionCodeContext = new FunctionCodeContext(Logger, functionCodeContext);

            return new CodeContextScope(() =>
            {
                return functionCodeContext = contexts.Count == 0 ? null : contexts.Pop();
            });
        }
    }
}