using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ColumnHeader : Node
    {
        public string Name { get; }
        public VariableDeclarationType Type { get; }

        public ColumnHeader(string name, VariableDeclarationType type, SourceReference reference) : base(reference)
        {
            Name = name;
            Type = type;
        }

        public static ColumnHeader Parse(string name, string typeName, SourceReference reference)
        {
            var type = VariableDeclarationType.Parse(typeName);
            return new ColumnHeader(name, type, reference);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
        }
    }
}