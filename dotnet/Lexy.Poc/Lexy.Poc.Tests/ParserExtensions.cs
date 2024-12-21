using System;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc
{
    public static class ParserExtensions
    {
        public static Components ParseComponents(this ILexyParser parser, string code)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            var codeLines = code.Split(Environment.NewLine);
            var context = parser.Parse(codeLines, false);

            return context.Components;
        }

        public static Function ParseFunction(this ILexyParser parser, string code) => parser.ParseComponent<Function>(code);
        public static Table ParseTable(this ILexyParser parser, string code) => parser.ParseComponent<Table>(code);
        public static Scenario ParseScenario(this ILexyParser parser, string code) => parser.ParseComponent<Scenario>(code);

        public static T ParseComponent<T>(this ILexyParser parser, string code) where T : RootComponent
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            var components = parser.ParseComponents(code);

            if (components.Count != 1)
            {
                throw new InvalidOperationException("Only 1 component expected. Actual: " + components.Count);
            }

            var first = components.First();
            if (!(first is T component))
            {
                throw new InvalidOperationException($"Component not a {typeof(T).Name}. Actual: {first?.GetType()}");
            }

            return component;
        }
    }
}