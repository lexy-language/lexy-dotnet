using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

public class EnumWriter : IComponentTokenWriter
{
    public GeneratedClass CreateCode(IComponentNode node)
    {
        if (node is not EnumDefinition enumDefinition) throw new InvalidOperationException("Component token not Function");

        var className = ClassNames.EnumClassName(enumDefinition.Name.Value);
        var members = WriteValues(enumDefinition);

        var enumNode = EnumDeclaration(className)
            .WithMembers(SeparatedList<EnumMemberDeclarationSyntax>(members))
            .WithModifiers(Modifiers.Public());

        return new GeneratedClass(enumDefinition, className, enumNode);
    }

    private SyntaxNodeOrToken[] WriteValues(EnumDefinition enumDefinition)
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