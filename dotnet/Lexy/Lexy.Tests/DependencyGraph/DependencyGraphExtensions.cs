using System;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.DependencyGraph;

public static class DependencyGraphExtensions
{
    public static Dependencies BuildGraph(this IServiceProvider serviceProvider, string code,
        bool throwException = true)
    {
        var(nodes, logger) = serviceProvider.ParseNodes(code);
        if (throwException) logger.AssertNoErrors();

        return DependencyGraphFactory.Create(nodes);
    }
}