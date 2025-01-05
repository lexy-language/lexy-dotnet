using System;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Compiler;

public static class LexyScript
{
    public static ExecutableFunction CompileFunction(this IServiceProvider serviceProvider, string code)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        var (rootNodeList, _) = serviceProvider.ParseNodes(code);

        var compiler = serviceProvider.GetRequiredService<ILexyCompiler>();
        var environment = compiler.Compile(rootNodeList);

        var firstOrDefault = rootNodeList.OfType<Function>().FirstOrDefault();
        return environment.GetFunction(firstOrDefault);
    }
}