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
        private CompilationResult compilationResult;
        private ExecutableFunction function;

        public CompileFunctionResult(ExecutableFunction function, CompilationResult compilationResult)
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
            var executionContext = compilationResult.CreateContext();
            return function.Run(executionContext, values);
        }
    }

    public static CompileFunctionResult CompileFunction(this IServiceProvider serviceProvider, string code)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        var (rootNodeList, _) = serviceProvider.ParseNodes(code);

        var compiler = serviceProvider.GetRequiredService<ILexyCompiler>();
        var environment = compiler.Compile(rootNodeList);

        var firstOrDefault = rootNodeList.OfType<Function>().FirstOrDefault();
        return new CompileFunctionResult(environment.GetFunction(firstOrDefault), environment);
    }
}