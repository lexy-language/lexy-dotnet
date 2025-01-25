using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class PowerFunctionCall : MethodFunctionCall<PowerFunction>
{
    protected override string ClassName => nameof(BuiltInNumberFunctions);
    protected override string MethodName => nameof(BuiltInNumberFunctions.Power);

    protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(PowerFunction powerFunction)
    {
        return SyntaxFactory.SeparatedList<ArgumentSyntax>(
            new SyntaxNodeOrToken[]
            {
                SyntaxFactory.Argument(
                    ExpressionSyntaxFactory.ExpressionSyntax(powerFunction.NumberExpression)),
                SyntaxFactory.Token(SyntaxKind.CommaToken),
                SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(powerFunction.PowerExpression))
            });
    }
}