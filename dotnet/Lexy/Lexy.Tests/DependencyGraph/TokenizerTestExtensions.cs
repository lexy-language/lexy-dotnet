using System;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.DependencyGraph;

public static class DependencyGraphExtensions
{
    public static Dependencies BuildGraph(this IServiceProvider serviceProvider, string code, bool throwException = true)
    {
        var parser = serviceProvider.GetRequiredService<ILexyParser>();
        var codeLines = code.Split(Environment.NewLine);
        var result = parser.Parse(codeLines, "tests", throwException);
        return DependencyGraphFactory.Create(result.RootNodes);
    }
}