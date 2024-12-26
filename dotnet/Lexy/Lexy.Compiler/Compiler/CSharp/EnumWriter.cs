using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    public class EnumWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node)
        {
            if (!(node is EnumDefinition enumDefinition))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var name = enumDefinition.Name.Value;
            var members = WriteValues(enumDefinition);

            var enumNode = EnumDeclaration(name)
                .WithMembers(SeparatedList<EnumMemberDeclarationSyntax>(members))
                .WithModifiers(Modifiers.Public());

            return new GeneratedClass(enumDefinition, name, enumNode);
        }

        private SyntaxNodeOrToken[] WriteValues(EnumDefinition enumDefinition)
        {
            var result = new List<SyntaxNodeOrToken>();
            foreach (var value in enumDefinition.Members)
            {
                if (result.Count > 0)
                {
                    result.Add(Token(SyntaxKind.CommaToken));
                }

                var declaration = EnumMemberDeclaration(value.Name);
                if (value.Value != null)
                {
                    declaration = declaration.WithEqualsValue(
                        EqualsValueClause(
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                            Literal((int) value.Value.NumberValue))));
                }
                result.Add(declaration);
            }

            return result.ToArray();
        }
    }
}