using System;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;

namespace Lexy.Tests.DependencyGraph;

public static class DependencyGraphExtensions
{
    public static async Task<Dependencies> BuildGraph(this IServiceProvider serviceProvider, string code,
        bool throwException = true)
    {
        var(nodes, logger, _) = await serviceProvider.ParseNodes(code);
        if (throwException) logger.AssertNoErrors();

        return DependencyGraphFactory.Create(nodes);
    }
}
