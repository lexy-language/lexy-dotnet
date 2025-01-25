using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Compiler;

public class CompilationEnvironment : ICompilationEnvironment
{
    private readonly IList<GeneratedClass> generatedTypes = new List<GeneratedClass>();
    private readonly IDictionary<string, ExecutableFunction> executables = new Dictionary<string, ExecutableFunction>();
    private readonly IDictionary<string, Type> enums = new Dictionary<string, Type>();
    private readonly IDictionary<string, Type> tables = new Dictionary<string, Type>();
    private readonly IDictionary<string, Type> types = new Dictionary<string, Type>();
    private readonly AssemblyLoadContext assemblyLoadContext;
    private readonly ILogger compilationLogger;
    private readonly ILogger<ExecutionContext> executionLogger;

    public string Namespace { get; } = $"Lexy.Runtime.{DateTime.Now:yyyyMMddHHmmss}";

    private Assembly assembly;

    public CompilationEnvironment(ILogger compilationLogger, ILogger<ExecutionContext> executionLogger)
    {
        this.compilationLogger = compilationLogger ?? throw new ArgumentNullException(nameof(compilationLogger));
        this.executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
        assemblyLoadContext = new AssemblyLoadContext(Namespace, true);
    }

    public void AddType(GeneratedClass generatedType)
    {
        generatedTypes.Add(generatedType);
    }

    public void CreateExecutables(MemoryStream dllStream)
    {
        LoadAssembly(dllStream);

        foreach (var generatedClass in generatedTypes)
        {
            CreateExecutable(generatedClass);
        }
    }

    public ExecutableFunction GetFunction(Function function)
    {
        return executables[function.NodeName];
    }

    public Type GetEnumType(string type)
    {
        return enums[type];
    }

    private void LoadAssembly(Stream dllStream)
    {
        if (dllStream.Position > 0)
        {
            dllStream.Seek(0, SeekOrigin.Begin);
        }

        assembly = assemblyLoadContext.LoadFromStream(dllStream);
    }

    private void CreateExecutable(GeneratedClass generatedClass)
    {
        switch (generatedClass.Node)
        {
            case Function function:
            {
                var instanceType = assembly.GetType(generatedClass.FullClassName);
                var executable = new ExecutableFunction(function, instanceType, this, executionLogger);

                executables.Add(generatedClass.Node.NodeName, executable);
                break;
            }
            case EnumDefinition _:
            {
                CreateExecutable(assembly, generatedClass, enums);
                break;
            }
            case Table _:
            {
                CreateExecutable(assembly, generatedClass, tables);
                break;
            }
            case TypeDefinition _:
            {
                CreateExecutable(assembly, generatedClass, types);
                break;
            }
            default:
            {
                throw new InvalidOperationException("Unknown generated type: " + generatedClass.Node.GetType());
            }
        }
    }

    private void CreateExecutable(Assembly assembly, GeneratedClass generatedClass,
        IDictionary<string, Type> dictionary)
    {
        var instanceType = assembly.GetType(generatedClass.FullClassName);

        dictionary.Add(generatedClass.Node.NodeName, instanceType);
    }

    public void Dispose()
    {
        assemblyLoadContext.Unload();
    }

    public void CreateAssembly(SyntaxNode syntax, CSharpCompilation compilation, ICompilationEnvironment environment)
    {
        string fullString = null;
        if (compilationLogger.IsEnabled(LogLevel.Debug))
        {
            fullString = syntax.ToFullString();
            compilationLogger.LogDebug(fullString);
        }

        using var dllStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var emitResult = compilation.Emit(dllStream, pdbStream);
        if (!emitResult.Success)
        {
            CompilationFailed(fullString ?? syntax.ToFullString(), emitResult);
        }

        environment.CreateExecutables(dllStream);
    }

    private void CompilationFailed(string code, EmitResult emitResult)
    {
        var compilationFailed = $"Compilation failed: {FormatCompilationErrors(emitResult.Diagnostics)}";

        compilationLogger.LogError(compilationFailed);

        throw new InvalidOperationException($"{compilationFailed}{Environment.NewLine}code: {code}");
    }

    private static string FormatCompilationErrors(ImmutableArray<Diagnostic> emitResult)
    {
        var stringWriter = new StringWriter();
        foreach (var diagnostic in emitResult)
        {
            stringWriter.WriteLine($"  {diagnostic}");
        }

        return stringWriter.ToString();
    }
}