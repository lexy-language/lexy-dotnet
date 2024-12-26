using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Poc.Core.Parser;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Poc.Core.Language
{
    public class FunctionName : Node
    {
        public string Value { get; private set; }

        public FunctionName(SourceReference reference) : base(reference)
        {
        }

        public void ParseName(string name)
        {
            Value = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            if (string.IsNullOrEmpty(Value))
            {
                context.Logger.Fail(Reference, $"Invalid function name: '{Value}'. Name should not be empty.");
            }
            if (!SyntaxFacts.IsValidIdentifier(Value))
            {
                context.Logger.Fail(Reference, $"Invalid function name: '{Value}'.");
            }
        }
    }
}