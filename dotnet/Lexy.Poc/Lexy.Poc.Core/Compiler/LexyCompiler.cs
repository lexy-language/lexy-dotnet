using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.RunTime;
using Lexy.Poc.Core.Transcribe;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public class LexyCompiler : ILexyCompiler
    {
        private readonly IExecutionEnvironment environment;
        private readonly ICompilerContext compilerContext;

        public LexyCompiler(ICompilerContext compilerContext, IExecutionEnvironment environment)
        {
            this.compilerContext = compilerContext ?? throw new ArgumentNullException(nameof(compilerContext));
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public CompilerResult Compile(Components components, Function function)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));
            if (function == null) throw new ArgumentNullException(nameof(function));

            var generateNodes = FunctionComponentAndDependencies(components, function);

            var code = GenerateCode(components, generateNodes);
            var assembly = CreateAssembly(code);

            environment.CreateExecutables(assembly);
            return environment.Result();
        }

        private List<IRootComponent> FunctionComponentAndDependencies(Components components, Function function)
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

        private string GenerateCode(Components components, List<IRootComponent> generateComponents)
        {
            var classWriter = new ClassWriter();
            classWriter.WriteLine($"using System.Collections.Generic;");
            classWriter.WriteLine($"using {typeof(IExecutionContext).Namespace};");
            classWriter.WriteLine();
            classWriter.OpenScope($"namespace {WriterCode.Namespace}");

            foreach (var component in generateComponents)
            {
                var writer = GetWriter(component);
                var generatedType = writer.CreateCode(classWriter, component, components);

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
}