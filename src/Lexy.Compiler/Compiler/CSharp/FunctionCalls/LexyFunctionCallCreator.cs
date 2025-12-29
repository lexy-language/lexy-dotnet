using System;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.FunctionCalls;

//LexyFunction(variable)
internal class LexyFunctionCallCreator : IFunctionCallCreator<LexyFunctionCallExpression>
{
    public bool Matches(LexyFunctionCallExpression expression) => true;

    public ExpressionSyntax CreateExpressionSyntax(LexyFunctionCallExpression expression)
    {
        if (expression.AutoMap)
        {
            throw new InvalidOperationException("AutoMap should be set to false in an expression.");
        }

        return RunFunction(expression.FunctionName, expression.ParameterName);
    }

    public static InvocationExpressionSyntax RunFunction(string functionName, string variableName)
    {
        var arguments = new SyntaxNodeOrToken[]
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variableName)),
            SyntaxFactory.Token(SyntaxKind.CommaToken),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(LexyCodeConstants.ContextVariable))
        };
        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(ClassNames.FunctionClassName(functionName)),
                    SyntaxFactory.IdentifierName(LexyCodeConstants.RunMethod)))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments)));;
    }
}