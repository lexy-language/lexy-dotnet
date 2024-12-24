using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    internal class Modifiers
    {
        public static SyntaxToken Public => SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        public static SyntaxToken Static => SyntaxFactory.Token(SyntaxKind.StaticKeyword);
        public static SyntaxTokenList PublicAsList => SyntaxTokenList.Create(Public);
        public static SyntaxTokenList StaticAsList => SyntaxTokenList.Create(Static);
    }
}