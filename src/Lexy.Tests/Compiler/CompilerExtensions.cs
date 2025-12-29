using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Language.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Compiler;

public static class CompilerExtensions
{
    public class CompileFunctionResult : IDisposable
    {
        private readonly ExecutableFunction function;
        private ICompilationResult compilationResult;

        public CompileFunctionResult(ExecutableFunction function, ICompilationResult compilationResult)
        {
            this.compilationResult = compilationResult;
            this.function = function;
        }

        public void Dispose()
        {
            compilationResult?.Dispose();
            compilationResult = null;
        }

        public FunctionResult Run(IDictionary<string, object> values = null)
        {
            return function.Run(values);
        }
    }

    public static CompileFunctionResult CompileFunction(this IServiceProvider serviceProvider, string code)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        var (componentNodes, logger) = serviceProvider.ParseNodes(code);
        if (logger.HasErrors())
        {
            throw new InvalidOperationException("Parsing failed: " + logger.FormatMessages());
        }

        var compiler = serviceProvider.GetRequiredService<ILexyCompiler>();
        var environment = compiler.Compile(componentNodes);

        var firstOrDefault = componentNodes.OfType<Function>().FirstOrDefault();
        return new CompileFunctionResult(environment.GetFunction(firstOrDefault), environment);
    }
}