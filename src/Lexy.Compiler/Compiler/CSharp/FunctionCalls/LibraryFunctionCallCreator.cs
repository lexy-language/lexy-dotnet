using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.FunctionCalls;

internal class LibraryFunctionCallCreator : IFunctionCallCreator<MemberFunctionCallExpression>
{
    public bool Matches(MemberFunctionCallExpression expression) => expression.FunctionCall is LibraryFunctionCall;

    public ExpressionSyntax CreateExpressionSyntax(MemberFunctionCallExpression expression)
    {
        var functionCall = expression.FunctionCall as LibraryFunctionCall
            ?? throw new InvalidOperationException("expression.FunctionCall should not be null.");

        return CallFunction(functionCall.FullTypeName, expression.Arguments);
    }

    private static InvocationExpressionSyntax CallFunction(IdentifierPath functionPath, IReadOnlyList<Expression> arguments)
    {
        var argumentsSyntax = GetArguments(arguments);
        return CallFunction(functionPath, argumentsSyntax);
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

    private static InvocationExpressionSyntax CallFunction(IdentifierPath functionPath, IEnumerable<SyntaxNodeOrToken> argumentsSyntax)
    {
        return SyntaxFactory.InvocationExpression(ClassNames.FunctionClassName(functionPath))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(argumentsSyntax.ToArray())));
    }
}