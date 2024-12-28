using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language
{
    public class ScenarioFunctionName : Node
    {
        public string Value { get; private set; }

        public INode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            Value = line.Tokens.TokenValue(1);
            return this;
        }

        public ScenarioFunctionName(SourceReference reference) : base(reference)
        {
        }

        public override string ToString() => Value;

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public bool IsEmpty() => string.IsNullOrEmpty(Value);
    }
}