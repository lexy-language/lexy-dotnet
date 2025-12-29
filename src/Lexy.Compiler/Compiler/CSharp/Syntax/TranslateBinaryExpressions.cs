using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;
using Lexy.RunTime.Libraries;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Compiler.CSharp.Syntax.Expressions;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

internal static class TranslateBinaryExpressions
{
    private static readonly IList<ExpressionOperator> ComparisonOperators = new[]
    {
        ExpressionOperator.Equals,
        ExpressionOperator.NotEqual,
        ExpressionOperator.GreaterThan,
        ExpressionOperator.GreaterThanOrEqual,
        ExpressionOperator.LessThan,
        ExpressionOperator.LessThanOrEqual,
    };

    private static readonly IDictionary<ExpressionOperator, SyntaxKind> TranslateOperators =
        new Dictionary<ExpressionOperator, SyntaxKind>
        {
            { ExpressionOperator.Addition, SyntaxKind.AddExpression },
            { ExpressionOperator.Subtraction, SyntaxKind.SubtractExpression },
            { ExpressionOperator.Multiplication, SyntaxKind.MultiplyExpression },
            { ExpressionOperator.Division, SyntaxKind.DivideExpression },
            { ExpressionOperator.Modulus, SyntaxKind.ModuloExpression },

            { ExpressionOperator.GreaterThan, SyntaxKind.GreaterThanExpression },
            { ExpressionOperator.GreaterThanOrEqual, SyntaxKind.GreaterThanOrEqualExpression },
            { ExpressionOperator.LessThan, SyntaxKind.LessThanExpression },
            { ExpressionOperator.LessThanOrEqual, SyntaxKind.LessThanOrEqualExpression },

            { ExpressionOperator.And, SyntaxKind.LogicalAndExpression },
            { ExpressionOperator.Or, SyntaxKind.LogicalOrExpression },
            { ExpressionOperator.Equals, SyntaxKind.EqualsExpression },
            { ExpressionOperator.NotEqual, SyntaxKind.NotEqualsExpression }
        };

    public static ExpressionSyntax TranslateBinaryExpression(BinaryExpression expression)
    {
        if (expression.LeftVariableType.Equals(PrimitiveType.String) &&
            expression.Operator == ExpressionOperator.Addition)
        {
            return TranslateStringAddition(expression);
        }
        if (IsStringComparison(expression))
        {
            return TranslateStringComparison(expression);
        }


        var kind = Translate(expression.Operator);
        return BinaryExpression(
            kind,
            ExpressionSyntax(expression.Left),
            ExpressionSyntax(expression.Right));
    }

    private static ExpressionSyntax TranslateStringAddition(BinaryExpression expression)
    {
        var kind = Translate(expression.Operator);
        var expressionSyntax = ExpressionSyntax(expression.Right);
        if (expression.RightVariableType.Equals(PrimitiveType.Date))
        {
            expressionSyntax = FormatDate(expressionSyntax);
        }
        else if (expression.RightVariableType.Equals(PrimitiveType.Boolean))
        {
            expressionSyntax = FormatBoolean(expressionSyntax);
        }
        else if (expression.RightVariableType is EnumType)
        {
            expressionSyntax = FormatEnum(expressionSyntax);
        }

        return BinaryExpression(
            kind,
            ExpressionSyntax(expression.Left),
            expressionSyntax);
    }

    private static InvocationExpressionSyntax FormatBoolean(ExpressionSyntax expressionSyntax)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expressionSyntax,
                        IdentifierName(nameof(ToString)))),
                IdentifierName(nameof(string.ToLowerInvariant))));
    }

    private static ExpressionSyntax FormatDate(ExpressionSyntax expressionSyntax)
    {
        expressionSyntax = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ClassNames.FullName(typeof(Lexy.RunTime.Libraries.Date)),
                    IdentifierName(nameof(Date.Format))))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList<ArgumentSyntax>(
                        Argument(expressionSyntax))));
        return expressionSyntax;
    }


    private static ExpressionSyntax FormatEnum(ExpressionSyntax expressionSyntax)
    {
        expressionSyntax = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Lexy"),
                            IdentifierName("RunTime")),
                        IdentifierName(nameof(EnumFunctions))),
                    IdentifierName(nameof(EnumFunctions.Format))))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(expressionSyntax))));
        return expressionSyntax;
    }

    private static ExpressionSyntax TranslateStringComparison(BinaryExpression expression)
    {
        var kind = Translate(expression.Operator);
        return BinaryExpression(
            kind,
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("String"),
                        IdentifierName("Compare")))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(ExpressionSyntax(expression.Left)),
                                Token(SyntaxKind.CommaToken),
                                Argument(ExpressionSyntax(expression.Right)),
                            }))),
            LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(0)));
    }

    private static bool IsStringComparison(BinaryExpression expression)
    {
        return expression.LeftVariableType.Equals(PrimitiveType.String)
               && expression.RightVariableType.Equals(PrimitiveType.String)
               && ComparisonOperators.Contains(expression.Operator);
    }

    private static SyntaxKind Translate(ExpressionOperator expressionOperator)
    {
        if (!TranslateOperators.TryGetValue(expressionOperator, out var result))
            throw new ArgumentOutOfRangeException(nameof(expressionOperator), expressionOperator, null);

        return result;
    }
}