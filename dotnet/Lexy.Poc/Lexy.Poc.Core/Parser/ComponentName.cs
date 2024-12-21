
namespace Lexy.Poc.Core.Parser
{
    internal class ComponentName
    {
        public string Parameter { get; }
        public string Name { get; }

        private ComponentName(string name, string parameter)
        {
            Parameter = parameter;
            Name = name;
        }

        public static ComponentName Parse(Line line, IParserContext context)
        {
            var valid = context.ValidateTokens<ComponentName>()
                .Count(2)
                .Keyword(0)
                .StringLiteral(1)
                .IsValid;

            if (!valid) return null;

            var parameter = line.TokenValue(1);
            if (context.Components.Contains(parameter))
            {
                context.Logger.Fail("Duplicated component name: '" + parameter + "'");
                return null;
            }

            return new ComponentName(line.TokenValue(0), parameter);
        }

        public override string ToString() => $"{Name} {Parameter}";
    }
}