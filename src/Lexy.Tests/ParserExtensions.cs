using System;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Tests;

public record ParseResult<T>(T Result, IParserLogger Logger);

public static class ParserExtensions
{
    public static ParseResult<RootNodeList> ParseNodes(this IServiceProvider serviceProvider, string code)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        var parser = serviceProvider.GetRequiredService<ILexyParser>();

        var codeLines = code.Split(Environment.NewLine);
        var context = parser.Parse(codeLines, "tests.lexy", new ParseOptions() {SuppressException = true});

        return new ParseResult<RootNodeList>(context.Nodes, context.Logger);
    }

    public static ParseResult<Function> ParseFunction(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Function>(code);
    }

    public static ParseResult<Table> ParseTable(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Table>(code);
    }

    public static ParseResult<Scenario> ParseScenario(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<Scenario>(code);
    }

    public static ParseResult<EnumDefinition> ParseEnum(this IServiceProvider serviceProvider, string code)
    {
        return serviceProvider.ParseNode<EnumDefinition>(code);
    }

    private static ParseResult<T> ParseNode<T>(this IServiceProvider serviceProvider, string code) where T : RootNode
    {
        var (nodes, logger) = serviceProvider.ParseNodes(code);

        var node = nodes.OfType<T>().FirstOrDefault();
        if (node == null)
        {
            throw new InvalidOperationException($"Node not a {typeof(T).Name}. Actual: {string.Join(", ", nodes.Select(value => value.GetType().Name).ToArray())}");
        }

        return new ParseResult<T>(node, logger);
    }
}