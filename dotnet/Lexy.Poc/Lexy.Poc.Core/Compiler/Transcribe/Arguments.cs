using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    internal static class Arguments
    {
        public static SyntaxNode Numeric(int value)
        {
            return SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(value)));
        }
    }
}