using System;
using System.Collections.Generic;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.TypeSystem.Functions;
using Lexy.RunTime.Libraries;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Generation.CSharp.FunctionCalls;

internal static class TableLookUpRowFunctionCallSyntax
{
    public static bool Matches(MemberFunctionCallExpression expression) => expression.State is LookUpRowFunctionCallState;

    public static ExpressionSyntax Create(MemberFunctionCallExpression expression)
    {
        var lookupFunction = expression.State as LookUpRowFunctionCallState
                             ?? throw new InvalidOperationException("expression.FunctionCall should be LookUpRowFunctionCall");

        var arguments = GetArguments(lookupFunction);

        return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ClassNames.FullName(typeof(Table)),
                        IdentifierName(nameof(Table.LookUpRow))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(arguments)));
    }

    private static List<SyntaxNodeOrToken> GetArguments(LookUpRowFunctionCallState lookupFunction)
    {
        var arguments = new List<SyntaxNodeOrToken>();
        if (lookupFunction.DiscriminatorExpression != null)
        {
            arguments.AddRange(collection: new SyntaxNodeOrToken[]
            {
                Arguments.String(lookupFunction.DiscriminatorColumn),
                Token(SyntaxKind.CommaToken),
                Argument(Expressions.ExpressionSyntax(lookupFunction.DiscriminatorExpression)),
                Token(SyntaxKind.CommaToken),
                Arguments.MemberAccessLambda("row", lookupFunction.DiscriminatorColumn),
                Token(SyntaxKind.CommaToken),
            });
        }
        arguments.AddRange(new SyntaxNodeOrToken[]
        {
            Arguments.String(lookupFunction.SearchValueColumn),
            Token(SyntaxKind.CommaToken),

            Argument(Expressions.ExpressionSyntax(lookupFunction.ValueExpression)),
            Token(SyntaxKind.CommaToken),
            Arguments.MemberAccessLambda("row",lookupFunction.SearchValueColumn),
            Token(SyntaxKind.CommaToken),

            Arguments.String(lookupFunction.TableName),
            Token(SyntaxKind.CommaToken),
            Arguments.MemberAccess(ClassNames.TableClassName(lookupFunction.TableName), "Values"),
            Token(SyntaxKind.CommaToken),

            Argument(IdentifierName(LexyCodeConstants.ContextVariable))
        });
        return arguments;
    }
}