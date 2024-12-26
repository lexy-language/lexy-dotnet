using System;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.Compiler
{
    public static class LexyScript
    {
        public static ExecutableFunction CompileFunction(this IServiceScope serviceScope, string code)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));
            if (code == null) throw new ArgumentNullException(nameof(code));

            var parser = serviceScope.ServiceProvider.GetRequiredService<ILexyParser>();
            var nodes = parser.ParseNodes(code);
            var function = nodes.GetSingleFunction();

            var compiler = serviceScope.ServiceProvider.GetRequiredService<ILexyCompiler>();
            var environment = compiler.Compile(nodes, function);
            return environment.GetFunction(function);
        }
    }
}