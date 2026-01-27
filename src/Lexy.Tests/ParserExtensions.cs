using System;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Logging;
using Lexy.RunTime;
using Microsoft.Extensions.DependencyInjection;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Tests;

public static class ParserExtensions
{
    public record ParseResult(ComponentNodeList Nodes, IParserLogger Logger, Dependencies Dependencies);

    public static async Task<ParseResult> ParseLines(this IServiceProvider serviceProvider, string[] lines)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var parser = serviceProvider.GetRequiredService<ILexyParser>();

        var context = await parser.ParseCode("tests.lexy", lines, new ParseOptions {SuppressException = true});

        return new ParseResult(context.Nodes, context.Logger, context.Dependencies);
    }

    public static async Task<ParseResult> ParseNodes(this IServiceProvider serviceProvider, string code)
    {
        var lines = code.Split(Environment.NewLine);
        return await serviceProvider.ParseLines(lines);
    }

    public static Task<ParseResult<Function>> ParseFunction(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Function>(code);
    }

    public static Task<ParseResult<Table>> ParseTable(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Table>(code);
    }

    public static Task<ParseResult<Scenario>> ParseScenario(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Scenario>(code);
    }

    public static Task<ParseResult<EnumDefinition>> ParseEnum(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<EnumDefinition>(code);
    }

    private static async Task<ParseResult<T>> ParseNode<T>(this IServiceProvider serviceProvider, string code) where T : ComponentNode
    {
        var (nodes, logger, _) = await serviceProvider.ParseNodes(code);

        var node = nodes.OfType<T>().FirstOrDefault();
        if (node == null)
        {
            throw new InvalidOperationException($"Node not a {typeof(T).Name}. Actual: {string.Join(", ", nodes.Select(value => value.GetType().Name).ToArray())}");
        }
        return new ParseResult<T>(node, logger);
    }
}
