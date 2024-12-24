using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class ValidationContext : IValidationContext
    {
        private class CodeContextScope : IDisposable
        {
            private readonly Func<IFunctionCodeContext> func;

            public CodeContextScope(Func<IFunctionCodeContext> func) => this.func = func;

            public void Dispose() => func();
        }

        public IParserContext ParserContext { get; }
        public IFunctionCodeContext FunctionCodeContext { get; private set; }
        public IParserLogger Logger => ParserContext.Logger;
        public Nodes Nodes => ParserContext.Nodes;

        public ValidationContext(IParserContext context)
        {
            ParserContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IDisposable CreateCodeContextScope()
        {
            if (FunctionCodeContext != null)
            {
                throw new InvalidOperationException("Already in a code scope. Only can enter scope once.");
            }

            FunctionCodeContext = new FunctionCodeContext(Nodes, Logger);
            return new CodeContextScope(() => FunctionCodeContext = null);
        }
    }
}