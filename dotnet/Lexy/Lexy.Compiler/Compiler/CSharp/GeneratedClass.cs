using Lexy.Poc.Core.Language;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    public class GeneratedClass
    {
        public IRootNode Node { get; }
        public string ClassName { get; }
        public string FullClassName => $"{LexyCodeConstants.Namespace}.{ClassName}";
        public MemberDeclarationSyntax Syntax { get; }

        public GeneratedClass(IRootNode node, string className, MemberDeclarationSyntax syntax)
        {
            Node = node;
            ClassName = className;
            Syntax = syntax;
        }
    }
}