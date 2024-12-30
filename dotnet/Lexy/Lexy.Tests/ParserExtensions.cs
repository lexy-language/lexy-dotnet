using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Tests;

public static class ParserExtensions
{
    public static RootNodeList ParseNodes(this ILexyParser parser, string code)
    {
        if (parser == null) throw new ArgumentNullException(nameof(parser));

        var codeLines = code.Split(Environment.NewLine);
        var context = parser.Parse(codeLines, "tests.lexy", false);

        return context.RootNodes;
    }

    public static Function ParseFunction(this ILexyParser parser, string code)
    {
        return parser.ParseNode<Function>(code);
    }

    public static Table ParseTable(this ILexyParser parser, string code)
    {
        return parser.ParseNode<Table>(code);
    }

    public static Scenario ParseScenario(this ILexyParser parser, string code)
    {
        return parser.ParseNode<Scenario>(code);
    }

    public static EnumDefinition ParseEnum(this ILexyParser parser, string code)
    {
        return parser.ParseNode<EnumDefinition>(code);
    }

    public static T ParseNode<T>(this ILexyParser parser, string code) where T : RootNode
    {
        if (parser == null) throw new ArgumentNullException(nameof(parser));

        var nodes = parser.ParseNodes(code);
        if (nodes.Count != 1) throw new InvalidOperationException("Only 1 node expected. Actual: " + nodes.Count);

        var first = nodes.First();
        if (!(first is T node))
            throw new InvalidOperationException($"Node not a {typeof(T).Name}. Actual: {first?.GetType()}");

        return node;
    }
}