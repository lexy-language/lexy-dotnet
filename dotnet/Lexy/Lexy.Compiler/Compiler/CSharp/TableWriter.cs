using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal class TableWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node)
        {
            if (!(node is Table table))
            {
                throw new InvalidOperationException("Root token not table");
            }

            var className = table.Name.Value;
            var rowName = $"{className}Row";

            var members = new List<MemberDeclarationSyntax>();
            members.Add(GenerateRowClass(rowName, table));
            members.Add(GenerateFields(rowName));
            members.Add(GenerateStaticConstructor(className, table, rowName));
            members.AddRange(GenerateProperties(rowName));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(Modifiers.Public())
                .WithMembers(List(members));

            return new GeneratedClass(node, className, classDeclaration);
        }

        private static ClassDeclarationSyntax GenerateRowClass(string rowName, Table table)
        {
            var properties = List<MemberDeclarationSyntax>(
                table.Header.Values
                    .Select(header =>
                        PropertyDeclaration(
                                Types.Syntax(header.Type),
                                Identifier(header.Name))
                            .WithModifiers(Modifiers.Public())
                            .WithAccessorList(Accessors.GetSet)));

            var rowClassDeclaration = ClassDeclaration(rowName)
                .WithModifiers(Modifiers.Public())
                .WithMembers(properties);

            return rowClassDeclaration;
        }

        private static FieldDeclarationSyntax GenerateFields(string rowName)
        {
            var fieldDeclaration = FieldDeclaration(
                VariableDeclaration(
                        GenericName(Identifier("List"))
                            .WithTypeArgumentList(
                                TypeArgumentList(SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(rowName)))))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("_value")))))
                    .WithModifiers(Modifiers.PrivateStatic());

            return fieldDeclaration;
        }

        private static ConstructorDeclarationSyntax GenerateStaticConstructor(string className, Table table, string rowName)
        {
            var rows = table.Rows.Select(row =>
                    ObjectCreationExpression(
                            IdentifierName(rowName))
                        .WithInitializer(
                            InitializerExpression(
                                SyntaxKind.ObjectInitializerExpression,
                                SeparatedList<ExpressionSyntax>(
                                    RowValues(row, table.Header)))));

            var declaration = ConstructorDeclaration(Identifier(className))
                .WithModifiers(Modifiers.Static())
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName("_value"),
                                    ObjectCreationExpression(
                                            GenericName(Identifier("List"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName(rowName)))))
                                        .WithInitializer(
                                            InitializerExpression(
                                                SyntaxKind.CollectionInitializerExpression,
                                                SeparatedList<ExpressionSyntax>(
                                                    rows
                                                    ))))))));

            return declaration;
        }

        private static SyntaxNodeOrToken[] RowValues(TableRow tableRow, TableHeader header)
        {
            var result = new List<SyntaxNodeOrToken>();
            for (var index = 0; index < header.Values.Count; index++)
            {
                var columnHeader = header.Values[index];
                var value = tableRow.Values[index];

                if (result.Count > 0)
                {
                    result.Add(Token(SyntaxKind.CommaToken));
                }

                result.Add(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(columnHeader.Name),
                        TokenValuesSyntax.Expression(value)));
            }
            return result.ToArray();
        }

        private static IEnumerable<PropertyDeclarationSyntax> GenerateProperties(string rowName)
        {
            yield return PropertyDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.IntKeyword)),
                        Identifier("Count"))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("_value"),
                                IdentifierName("Count"))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));

            yield return
                PropertyDeclaration(
                        GenericName(
                                Identifier("IReadOnlyList"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(rowName)))),
                        Identifier("Values"))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            IdentifierName("_value")))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));
        }
    }
}