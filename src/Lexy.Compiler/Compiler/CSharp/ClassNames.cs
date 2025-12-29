using System;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp;

public static class ClassNames
{
    public static ExpressionSyntax FullName(Type type)
    {
        var path = IdentifierPath.Parse(type.Namespace, type.Name);
        return FunctionClassName(path);
    }

    public static ExpressionSyntax FunctionClassName(IdentifierPath functionPath)
    {
        ExpressionSyntax expression = null;

        for (var index = 0 ; index < functionPath.Parts ; index++)
        {
            var part = functionPath.Path[index];
            if (expression == null)
            {
                expression = SyntaxFactory.IdentifierName(part);
            }
            else
            {
                expression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    SyntaxFactory.IdentifierName(part));
            }
        }

        return expression;
    }

    public static string FunctionClassName(string functionName)
    {
        return Normalize(functionName, LexyCodeConstants.FunctionClassPrefix);
    }

    public static string TableClassName(string tableTypeName)
    {
        return Normalize(tableTypeName, LexyCodeConstants.TableClassPrefix);
    }

    public static string EnumClassName(string enumName)
    {
        return Normalize(enumName, LexyCodeConstants.EnumClassPrefix);
    }

    public static string TypeClassName(string enumName)
    {
        return Normalize(enumName, LexyCodeConstants.TypeClassPrefix);
    }

    private static string Normalize(string functionName, string functionClassPrefix)
    {
        var nameBuilder = new StringBuilder(functionClassPrefix);
        foreach (var @char in functionName.Where(ValidCharacter)) nameBuilder.Append(@char);

        return nameBuilder.ToString();
    }

    private static bool ValidCharacter(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_';
    }
}