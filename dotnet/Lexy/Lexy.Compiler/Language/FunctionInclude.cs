using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionInclude : Node
    {
        public string Type { get; }
        public string Name { get; }

        private FunctionInclude(string name, string type, SourceReference reference) : base(reference)
        {
            Type = type;
            Name = name;
        }

        public static FunctionInclude Parse(IParserContext context)
        {
            var valid = context.ValidateTokens<FunctionInclude>()
                .Count(2)
                .StringLiteral(0)
                .StringLiteral(1)
                .IsValid;

            if (!valid) return null;

            var tokens = context.CurrentLine.Tokens;

            var name = tokens.TokenValue(1);
            var type = tokens.TokenValue(0);
            var reference = context.LineStartReference();

            return new FunctionInclude(name, type, reference);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            var table = context.Nodes.GetTable(Name);
            if (table == null)
            {
                context.Logger.Fail(Reference, $"Invalid table name: '{Name}'");
                return;
            }

            var variableType = new TableType(Name, table);
            context.FunctionCodeContext.RegisterVariableAndVerifyUnique(Reference, Name, variableType);
        }
    }
}