using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class RoundFunctionCall : MethodFunctionCall<RoundFunction>
{
    protected override string ClassName => nameof(BuiltInNumberFunctions);
    protected override string MethodName => nameof(BuiltInNumberFunctions.Round);

    protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(RoundFunction roundFunction)
    {
        return SyntaxFactory.SeparatedList<ArgumentSyntax>(
            new SyntaxNodeOrToken[]
            {
                SyntaxFactory.Argument(
                    ExpressionSyntaxFactory.ExpressionSyntax(roundFunction.NumberExpression)),
                SyntaxFactory.Token(SyntaxKind.CommaToken),
                SyntaxFactory.Argument(
                    ExpressionSyntaxFactory.ExpressionSyntax(roundFunction.DigitsExpression))
            });
    }
}