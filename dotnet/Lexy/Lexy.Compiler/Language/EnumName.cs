using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Poc.Core.Language
{
    public class EnumName : Node
    {
        public string Value { get; private set; }

        public EnumName(SourceReference sourceReference) : base(sourceReference)
        {
        }

        public void ParseName(string parameter)
        {
            Value = parameter;
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            if (string.IsNullOrEmpty(Value))
            {
                context.Logger.Fail(Reference, $"Invalid enum name: {Value}. Name should not be empty.");
            }
            if (!SyntaxFacts.IsValidIdentifier(Value))
            {
                context.Logger.Fail(Reference, $"Invalid enum name: {Value}.");
            }
        }
    }
}