using System;
using System.IO;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public abstract class Expression : Node
    {
        public ExpressionSource Source { get; }

        protected Expression(ExpressionSource source, SourceReference reference) : base(reference)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            foreach (var token in Source.Tokens)
            {
                writer.Write(token.Value);
            }
            return writer.ToString();
        }

        public abstract VariableType DeriveType(IValidationContext context);
    }
}