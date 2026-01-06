using System.Collections.Generic;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.FunctionCalls;

//LexyFunction(variable)
internal static class LexyFunctionCallSyntax
{
    public static bool Matches(LexyFunctionCallExpression expression) => true;

    public static ExpressionSyntax Create(LexyFunctionCallExpression expression)
    {
        var lexyFunctionCall = Assert.Is<LexyFunctionCall>(expression.FunctionCall, "expression.FunctionCall");

        return RunFunction(expression.FunctionName, lexyFunctionCall.ArgumentExpressions);
    }

    public static InvocationExpressionSyntax RunFunction(string functionName, string variableName)
    {
        var argumentsSyntax = new SyntaxNodeOrToken[]
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variableName)),
            SyntaxFactory.Token(SyntaxKind.CommaToken),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(LexyCodeConstants.ContextVariable))
        };

        return InvocationExpressionSyntax(LexyCodeConstants.RunMethod, functionName, argumentsSyntax);
    }

    private static InvocationExpressionSyntax RunFunction(string functionName, IReadOnlyList<Expression> arguments)
    {
        var argumentsSyntax = GetArguments(arguments);
        argumentsSyntax.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
        argumentsSyntax.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(LexyCodeConstants.ContextVariable)));

        return InvocationExpressionSyntax(LexyCodeConstants.RunMethod, functionName, argumentsSyntax);
    }

    private static InvocationExpressionSyntax InvocationExpressionSyntax(string runMethodName, string functionName, IReadOnlyList<SyntaxNodeOrToken> argumentsSyntax)
    {
        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(ClassNames.FunctionClassName(functionName)),
                    SyntaxFactory.IdentifierName(runMethodName)))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(argumentsSyntax)));
    }

    private static List<SyntaxNodeOrToken> GetArguments(IReadOnlyList<Expression> arguments)
    {
        var argumentsSyntax = new List<SyntaxNodeOrToken>();

        foreach (var argument in arguments)
        {
            if (argumentsSyntax.Count > 0)
            {
                argumentsSyntax.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
            }

            argumentsSyntax.Add(SyntaxFactory.Argument(Expressions.ExpressionSyntax(argument)));
        }

        return argumentsSyntax;
    }
}