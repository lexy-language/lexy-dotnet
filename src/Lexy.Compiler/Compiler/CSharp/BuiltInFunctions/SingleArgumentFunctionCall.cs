using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal abstract class SingleArgumentFunctionCall<TExpressionFunction> : MethodFunctionCall<TExpressionFunction>
    where TExpressionFunction : SingleArgumentFunction
{
    protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(TExpressionFunction powerFunction)
    {
        return SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.Argument(
                ExpressionSyntaxFactory.ExpressionSyntax(powerFunction.ValueExpression)));
    }
}