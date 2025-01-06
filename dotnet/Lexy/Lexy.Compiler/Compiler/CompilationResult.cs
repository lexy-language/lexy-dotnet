using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Functions;
using Lexy.RunTime;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Compiler;

public class CompilationResult : IDisposable
{
    private readonly IDictionary<string, Type> enums;
    private readonly IDictionary<string, ExecutableFunction> executables;
    private readonly CompilationEnvironment environment;
    private readonly ILogger<ExecutionContext> logger;

    public CompilationResult(IDictionary<string, ExecutableFunction> executables, IDictionary<string, Type> enums,
        CompilationEnvironment environment, ILogger<ExecutionContext> logger)
    {
        this.executables = executables;
        this.enums = enums;
        this.environment = environment;
        this.logger = logger;
    }

    public ExecutableFunction GetFunction(Function function)
    {
        return executables[function.NodeName];
    }

    public Type GetEnumType(string type)
    {
        return enums[type];
    }

    public IExecutionContext CreateContext()
    {
        return new ExecutionContext(logger);
    }

    public void Dispose()
    {
        environment.Dispose();
    }
}