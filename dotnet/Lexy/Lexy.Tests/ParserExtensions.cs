using System;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc
{
    public static class ParserExtensions
    {
        public static Nodes ParseNodes(this ILexyParser parser, string code)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            var codeLines = code.Split(Environment.NewLine);
            var context = parser.Parse(codeLines, "tests.lexy", false);

            return context.Nodes;
        }

        public static Function ParseFunction(this ILexyParser parser, string code) => parser.ParseNode<Function>(code);
        public static Table ParseTable(this ILexyParser parser, string code) => parser.ParseNode<Table>(code);
        public static Scenario ParseScenario(this ILexyParser parser, string code) => parser.ParseNode<Scenario>(code);
        public static EnumDefinition ParseEnum(this ILexyParser parser, string code) => parser.ParseNode<EnumDefinition>(code);

        public static T ParseNode<T>(this ILexyParser parser, string code) where T : RootNode
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            var nodes = parser.ParseNodes(code);
            if (nodes.Count != 1)
            {
                throw new InvalidOperationException("Only 1 node expected. Actual: " + nodes.Count);
            }

            var first = nodes.First();
            if (!(first is T node))
            {
                throw new InvalidOperationException($"Node not a {typeof(T).Name}. Actual: {first?.GetType()}");
            }

            return node;
        }
    }
}