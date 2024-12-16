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
    public class LexyCompilerContext
    {
        public ExecutionEnvironment ExecutionEnvironment { get; }
        public ILogger Logger { get; }

        public LexyCompilerContext(ExecutionEnvironment executionEnvironment)
        {
            ExecutionEnvironment = executionEnvironment;
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());

            Logger = factory.CreateLogger("Program");
        }
    }

    public class LexyCompiler
    {
        public ExecutionEnvironment Compile(TypeSystem typeSystem, Function function)
        {
            if (typeSystem == null) throw new ArgumentNullException(nameof(typeSystem));
            if (function == null) throw new ArgumentNullException(nameof(function));

            var environment = new ExecutionEnvironment();
            var context = new LexyCompilerContext(environment);

            var generateNodes = new List<IRootToken> { function };
            generateNodes.AddRange(function.GetDependencies(typeSystem));
            
            var codeWriter = new StringBuilder();

            foreach (var generateNode in generateNodes)
            {
                var writer = GetWriter(generateNode);
                var generatedType = writer.CreateCode(generateNode, typeSystem);

                codeWriter.Append(generatedType.Code);
                environment.AddType(generatedType);
            }

            var code = codeWriter.ToString();
            context.Logger.LogDebug("Compile code: " + code);
            Console.WriteLine("Compile code: " + code);

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
                                                    Environment.NewLine + "code: " + codeWriter);

            var assembly = Assembly.Load(dllStream.ToArray());

            environment.CreateExecutables(assembly);
            return environment;
        }

        private static IRootTokenWriter GetWriter(IRootToken rootToken)
        {
            switch (rootToken)
            {
                case Function _:
                    return new FunctionWriter();
                case EnumDefinition _:
                    return new EnumWriter();
                case Scenario _:
                    return null;
            }

            throw new InvalidOperationException("No writer defined: " + rootToken.GetType());
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

    public class ExecutionEnvironment
    {
        private readonly IList<GeneratedClass> generatedTypes = new List<GeneratedClass>();
        private readonly IDictionary<string, ExecutableFunction> executables = new Dictionary<string, ExecutableFunction>();
        private readonly IDictionary<string, Type> enums = new Dictionary<string, Type>();

        public void CreateExecutables(Assembly assembly)
        {
            foreach (var generatedClass in generatedTypes)
            {
                if (generatedClass.Token is Function)
                {
                    var instance = assembly.CreateInstance(generatedClass.FullClassName);
                    var executable = new ExecutableFunction(instance);

                    executables.Add(generatedClass.Token.TokenName, executable);
                }
                else if (generatedClass.Token is EnumDefinition)
                {
                    var enumType = assembly.GetType(generatedClass.FullClassName);
                    enums.Add(generatedClass.Token.TokenName, enumType);
                }
                else
                {
                    throw new InvalidOperationException("Unknown generated type: " + generatedClass.Token.GetType());
                }
            }
        }

        internal void AddType(GeneratedClass generatedType)
        {
            generatedTypes.Add(generatedType);
        }

        public ExecutableFunction GetFunction(Function function)
        {
            return executables[function.TokenName];
        }

        public bool ContainsEnum(string type)
        {
            return enums.ContainsKey(type);
        }

        public Type GetEnumType(string type)
        {
            return enums[type];
        }
    }

    internal interface IRootTokenWriter
    {
        GeneratedClass CreateCode(IRootToken generateNode, TypeSystem typeSystem);
    }
}