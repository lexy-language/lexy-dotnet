using Lexy.Compiler.Language;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler;

public class GeneratedClass
{
    public IComponentNode Node { get; }
    public string ClassName { get; }
    public string FullClassName => $"{LexyCodeConstants.Namespace}.{ClassName}";
    public MemberDeclarationSyntax Syntax { get; }

    public GeneratedClass(IComponentNode node, string className, MemberDeclarationSyntax syntax)
    {
        Node = node;
        ClassName = className;
        Syntax = syntax;
    }
}