using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public class LexyCompiler : ILexyCompiler
    {
        private readonly ICompilerContext compilerContext;

        public LexyCompiler(ICompilerContext compilerContext)
        {
            this.compilerContext = compilerContext ?? throw new ArgumentNullException(nameof(compilerContext));
        }

        public ExecutionEnvironment Compile(Components components, Function function)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));
            if (function == null) throw new ArgumentNullException(nameof(function));

            var environment = new ExecutionEnvironment();

            var generateNodes = FunctionComponentAndDependencies(components, function);

            var code = GenerateCode(components, generateNodes, environment);
            var assembly = CreateAssembly(code);

            environment.CreateExecutables(assembly);
            return environment;
        }

        private static List<IRootComponent> FunctionComponentAndDependencies(Components components, Function function)
        {
            var generateNodes = new List<IRootComponent> { function };
            generateNodes.AddRange(function.GetDependencies(components));
            return generateNodes;
        }

        private Assembly CreateAssembly(string code)
        {
            var compilation = CreateCSharp(code);

            using var dllStream = new MemoryStream();
            using var pdbStream = new MemoryStream();

            var emitResult = compilation.Emit(dllStream, pdbStream);
            if (!emitResult.Success)
            {
                CompilationFailed(code, emitResult);
            }

            return Assembly.Load(dllStream.ToArray());
        }

        private static CSharpCompilation CreateCSharp(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = GetDllReferences();

            return CSharpCompilation.Create(
                $"{WriterCode.Namespace}.{Guid.NewGuid():D}",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        private void CompilationFailed(string code, EmitResult emitResult)
        {
            var compilationFailed = "Compilation failed: " +
                                    FormatCompilationErrors(emitResult.Diagnostics) +
                                    Environment.NewLine + "code: " + code;

            compilerContext.Logger.LogError(compilationFailed);
            throw new InvalidOperationException(compilationFailed);
        }

        private static List<MetadataReference> GetDllReferences()
        {
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(FunctionResult).Assembly.Location)
            };

            Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .ToList()
                .ForEach(reference =>
                    references.Add(MetadataReference.CreateFromFile(Assembly.Load(reference).Location)));
            return references;
        }

        private string GenerateCode(Components components, List<IRootComponent> generateNodes, ExecutionEnvironment environment)
        {
            var classWriter = new ClassWriter();
            classWriter.WriteLine($"using System.Collections.Generic;");
            classWriter.OpenScope($"namespace {WriterCode.Namespace}");

            foreach (var generateNode in generateNodes)
            {
                var writer = GetWriter(generateNode);
                var generatedType = writer.CreateCode(classWriter, generateNode, components);

                environment.AddType(generatedType);
            }

            classWriter.CloseScope();

            var code = classWriter.ToString();
            compilerContext.Logger.LogDebug("Compile code: " + Environment.NewLine + code);
            return code;
        }

        private static IRootTokenWriter GetWriter(IRootComponent rootComponent)
        {
            return rootComponent switch
            {
                Function _ => new FunctionWriter(),
                EnumDefinition _ => new EnumWriter(),
                Table _ => new TableWriter(),
                Scenario _ => null,
                _ => throw new InvalidOperationException("No writer defined: " + rootComponent.GetType())
            };
        }

        private string FormatCompilationErrors(ImmutableArray<Diagnostic> emitResult)
        {
            var stringWriter = new StringWriter();
            foreach (var diagnostic in emitResult)
            {
                stringWriter.WriteLine("  " + diagnostic);
            }
            return stringWriter.ToString();
        }
    }
}