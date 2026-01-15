using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.RunTime;
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

    public static async Task<CompileFunctionResult> CompileFunction(this IServiceProvider serviceProvider, string code)
    {
        Assert.NotNull(code, nameof(code));

        var (componentNodes, logger, _) = await serviceProvider.ParseNodes(code);
        if (logger.HasErrors())
        {
            throw new InvalidOperationException("Parsing failed: " + logger.FormatMessages());
        }

        var compiler = serviceProvider.GetRequiredService<ILexyCompiler>();
        var environment = compiler.Compile(componentNodes);

        var functionNode = GetFunctionNode(componentNodes);

        Assert.NotNull(functionNode, nameof(functionNode));

        return new CompileFunctionResult(environment.GetFunction(functionNode), environment);
    }

    private static Function GetFunctionNode(ComponentNodeList componentNodes)
    {
        var node = componentNodes.FirstOrDefault(node => node is Function or Scenario);
        return node is Function function ? function : (node as Scenario)?.Function;
    }
}
