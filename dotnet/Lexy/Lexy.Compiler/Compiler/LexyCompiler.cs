using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lexy.Compiler.Compiler.CSharp;
using Lexy.Compiler.Language;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler;

public class LexyCompiler : ILexyCompiler
{
    private readonly ILogger<LexyCompiler> compilationLogger;
    private readonly ILogger<ExecutionContext> executionLogger;

    public LexyCompiler(ILogger<LexyCompiler> logger, ILogger<ExecutionContext> executionLogger)
    {
        this.compilationLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
    }

    public CompilationResult Compile(IEnumerable<IRootNode> nodes)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));

        var environment = new CompilationEnvironment(compilationLogger, executionLogger);
        try
        {
            var syntaxNode = GenerateSyntaxNode(nodes, environment);
            var compilation = CreateSyntaxTree(syntaxNode, environment);
            environment.CreateAssembly(syntaxNode, compilation, environment);

            return environment.Result();
        }
        catch (Exception e)
        {
            compilationLogger.LogError(e, "Exception occurred during compilation");
            throw;
        }
    }


    private static CSharpCompilation CreateSyntaxTree(SyntaxNode root, ICompilationEnvironment compilationEnvironment)
    {
        var syntaxTree = SyntaxTree(root);
        var references = GetDllReferences();

        return CSharpCompilation.Create(
            compilationEnvironment.Namespace,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static List<MetadataReference> GetDllReferences()
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ExecutionContext).Assembly.Location)
        };

        Assembly.GetEntryAssembly().GetReferencedAssemblies()
            .ToList()
            .ForEach(reference =>
                references.Add(MetadataReference.CreateFromFile(Assembly.Load(reference).Location)));

        return references;
    }

    private SyntaxNode GenerateSyntaxNode(IEnumerable<IRootNode> generateNodes, ICompilationEnvironment compilationEnvironment)
    {
        var root = GenerateCompilationUnit(generateNodes, compilationEnvironment);

        return root.NormalizeWhitespace();
    }

    private CompilationUnitSyntax GenerateCompilationUnit(IEnumerable<IRootNode> generateNodes,
        ICompilationEnvironment compilationEnvironment)
    {
        var members = generateNodes
            .Select(value => GenerateMember(value, compilationEnvironment))
            .ToList();

        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(LexyCodeConstants.Namespace))
            .WithMembers(List(members));

        var root = CompilationUnit()
            .WithUsings(List(
                new[]
                {
                    Using("System"),
                    Using("System.Collections.Generic"),
                    Using(typeof(IExecutionContext).Namespace)
                }))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
        return root;
    }

    private MemberDeclarationSyntax GenerateMember(IRootNode node, ICompilationEnvironment environment)
    {
        var writer = CSharpCode.GetWriter(node);

        var generatedType = writer.CreateCode(node);

        environment.AddType(generatedType);

        return generatedType.Syntax;
    }

    private static UsingDirectiveSyntax Using(string ns)
    {
        return UsingDirective(ParseName(ns));
    }
}