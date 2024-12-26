using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal static class Modifiers
    {
        private static SyntaxToken PublicToken() => Token(SyntaxKind.PublicKeyword);
        private static SyntaxToken PrivateToken() => Token(SyntaxKind.PrivateKeyword);
        private static SyntaxToken StaticToken() => Token(SyntaxKind.StaticKeyword);

        public static SyntaxTokenList Public() => TokenList(PublicToken());
        public static SyntaxTokenList Static() => TokenList(StaticToken());
        public static SyntaxTokenList Private() => TokenList(PrivateToken());
        public static SyntaxTokenList PrivateStatic() => TokenList(PrivateToken(), StaticToken());
    }
}