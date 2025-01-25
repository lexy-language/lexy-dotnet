using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal abstract class NoArgumentFunctionCall : FunctionCall<NoArgumentFunction>
{
    protected abstract string ClassName { get; }
    protected abstract string MethodName { get; }

    public override ExpressionSyntax CallExpressionSyntax(NoArgumentFunction _)
    {
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(ClassName),
                SyntaxFactory.IdentifierName(MethodName)));
    }
}