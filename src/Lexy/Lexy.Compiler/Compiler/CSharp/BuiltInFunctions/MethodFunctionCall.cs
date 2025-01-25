using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal abstract class MethodFunctionCall<TExpressionFunction> : FunctionCall<TExpressionFunction>
    where TExpressionFunction : ExpressionFunction
{
    protected abstract string ClassName { get; }
    protected abstract string MethodName { get; }

    public override ExpressionSyntax CallExpressionSyntax(TExpressionFunction expressionFunction)
    {
        var arguments = GetArguments(expressionFunction);

        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(ClassName),
                    SyntaxFactory.IdentifierName(MethodName)))
            .WithArgumentList(SyntaxFactory.ArgumentList(arguments));
    }

    protected abstract SeparatedSyntaxList<ArgumentSyntax> GetArguments(TExpressionFunction powerFunction);
}