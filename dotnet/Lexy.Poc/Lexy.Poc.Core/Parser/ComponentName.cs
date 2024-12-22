
namespace Lexy.Poc.Core.Parser
{
    internal class ComponentName
    {
        public string Name { get; }
        public string Keyword { get; }

        private ComponentName(string keyword, string name)
        {
            Name = name;
            Keyword = keyword;
        }

        public static ComponentName Parse(Line line, IParserContext context)
        {
            var tokens = line.Tokens;
            if (tokens.Length < 1 && tokens.Length > 2) return null;

            var valid = context.ValidateTokens<ComponentName>()
                .Keyword(0)
                .IsValid;

            if (!valid) return null;

            var keyword = tokens.TokenValue(0);
            if (tokens.Length == 1)
            {
                return new ComponentName(keyword, null);
            }

            valid = context.ValidateTokens<ComponentName>()
                .StringLiteral(1)
                .IsValid;

            if (!valid) return null;

            var parameter = tokens.TokenValue(1);
            if (context.Components.Contains(parameter))
            {
                context.Logger.Fail($"Duplicated component name: '{parameter}'");
                return null;
            }

            return new ComponentName(keyword, parameter);
        }

        public override string ToString() => $"{Keyword} {Name}";
    }
}