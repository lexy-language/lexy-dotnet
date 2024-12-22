using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class EnumDefinition : RootComponent
    {
        public Comments Comments { get; } = new Comments();
        public EnumName Name { get; } = new EnumName();

        public override string ComponentName => Name.Value;

        public IList<EnumMember> Members { get; } = new List<EnumMember>();

        private EnumDefinition(string name)
        {
            Name.ParseName(name);
        }

        internal static EnumDefinition Parse(ComponentName name)
        {
            return new EnumDefinition(name.Name);
        }

        public override IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return this;

            if (line.IsComment())
            {
                Comments.Parse(context);
            }
            else
            {
                var assignment = EnumMember.Parse(context);
                Members.Add(assignment);
            }
            return this;
        }
    }
}