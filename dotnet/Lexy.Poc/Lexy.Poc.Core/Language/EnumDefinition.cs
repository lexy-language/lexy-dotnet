using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class EnumDefinition : RootComponent
    {
        public Comments Comments { get; } = new Comments();
        public EnumName Name { get; } = new EnumName();

        public override string ComponentName => Name.Value;

        public IList<AssignmentDefinition> Assignments { get; } = new List<AssignmentDefinition>();

        private EnumDefinition(string name)
        {
            Name.ParseName(name);
        }

        internal static EnumDefinition Parse(ComponentName name)
        {
            return new EnumDefinition(name.Parameter);
        }

        public override IComponent Parse(ParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return this;

            if (line.IsComment())
            {
                Comments.Parse(context);
            }
            else
            {
                var assignment = AssignmentDefinition.Parse(context);
                Assignments.Add(assignment);
            }
            return this;
        }
    }
}