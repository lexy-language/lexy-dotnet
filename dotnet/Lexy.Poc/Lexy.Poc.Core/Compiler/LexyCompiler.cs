using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public class LexyCompiler
    {
        public ExecutionEnvironment Compile(Components components, Function function)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));
            if (function == null) throw new ArgumentNullException(nameof(function));

            var environment = new ExecutionEnvironment();
            var context = new LexyCompilerContext(environment);

            var generateNodes = new List<IRootComponent> { function };
            generateNodes.AddRange(function.GetDependencies(components));
            
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
            context.Logger.LogDebug(code + "Compile code: ");

            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(FunctionResult).Assembly.Location)
            };

            Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .ToList()
                .ForEach(reference =>
                    references.Add(MetadataReference.CreateFromFile(Assembly.Load(reference).Location)));

            var compilation = CSharpCompilation.Create(
            WriterCode.Namespace + "." + Guid.NewGuid().ToString("D"),
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var dllStream = new MemoryStream();
            using var pdbStream = new MemoryStream();

            var emitResult = compilation.Emit(dllStream, pdbStream);
            if (!emitResult.Success)
                throw new InvalidOperationException("Compilation failed: " +
                                                    FormatCompilationErrors(emitResult.Diagnostics) +
                                                    Environment.NewLine + "code: " + classWriter);

            var assembly = Assembly.Load(dllStream.ToArray());

            environment.CreateExecutables(assembly);
            return environment;
        }

        private static IRootTokenWriter GetWriter(IRootComponent rootComponent)
        {
            switch (rootComponent)
            {
                case Function _:
                    return new FunctionWriter();
                case EnumDefinition _:
                    return new EnumWriter();
                case Table _:
                    return new TableWriter();
                case Scenario _:
                    return null;
            }

            throw new InvalidOperationException("No writer defined: " + rootComponent.GetType());
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