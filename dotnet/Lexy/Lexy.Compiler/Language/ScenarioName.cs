using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioName : Node
    {
        public string Value { get; private set; } = Guid.NewGuid().ToString("D");

        public void ParseName(string parameter)
        {
            Value = parameter;
        }

        public ScenarioName(SourceReference reference) : base(reference)
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
    }
}