using System;
using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Enums;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Generation.CSharp.Components;

public static class EnumClass
{
    public static GeneratedClass CreateCode(EnumDefinition enumDefinition)
    {
        Assert.NotNull(enumDefinition, nameof(enumDefinition));

        var className = ClassNames.EnumClassName(enumDefinition.Name);
        var members = WriteValues(enumDefinition);

        var enumNode = EnumDeclaration(className)
            .WithMembers(SeparatedList<EnumMemberDeclarationSyntax>(members))
            .WithModifiers(Modifiers.Public());

        return new GeneratedClass(enumDefinition, className, enumNode);
    }

    private static SyntaxNodeOrToken[] WriteValues(EnumDefinition enumDefinition)
    {
        var result = new List<SyntaxNodeOrToken>();
        foreach (var value in enumDefinition.Members)
        {
            if (result.Count > 0) result.Add(Token(SyntaxKind.CommaToken));

            var declaration = EnumMemberDeclaration(value.Name)
                .WithEqualsValue(
                    EqualsValueClause(
                        LiteralExpression(SyntaxKind.NumericLiteralExpression,
                            Literal(value.NumberValue))));

            result.Add(declaration);
        }

        return result.ToArray();
    }
}
