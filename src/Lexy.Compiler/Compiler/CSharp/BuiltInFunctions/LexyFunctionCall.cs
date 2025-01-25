using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class LexyFunctionCall : FunctionCall<LexyFunction>
{
    public override ExpressionSyntax CallExpressionSyntax(LexyFunction expression)
    {
        return RunFunction(expression.FunctionName, expression.VariableName);
    }

    public static InvocationExpressionSyntax RunFunction(string functionName, string variableName)
    {
        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(ClassNames.FunctionClassName(functionName)),
                    SyntaxFactory.IdentifierName(LexyCodeConstants.RunMethod)))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variableName)),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(LexyCodeConstants.ContextVariable))
                        })));
    }
}