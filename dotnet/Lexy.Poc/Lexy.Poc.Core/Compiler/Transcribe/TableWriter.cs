using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Poc.Core.Compiler.Transcribe.LexySyntaxFactory;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    internal class TableWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node, Nodes nodes)
        {
            if (!(node is Table table))
            {
                throw new InvalidOperationException("Root token not table");
            }

            var className = table.Name.Value;
            var rowName = $"{className}Row";

            var members = new List<MemberDeclarationSyntax>();
            members.Add(GenerateRowClass(nodes, rowName, table));
            members.Add(GenerateFields(rowName));
            members.Add(GenerateStaticConstructor(className, table, rowName));
            members.AddRange(GenerateProperties(rowName));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(Modifiers.PublicAsList)
                .WithMembers(List(members));

            return new GeneratedClass(node, className, classDeclaration);
        }

        private static ClassDeclarationSyntax GenerateRowClass(Nodes nodes, string rowName, Table table)
        {
            var properties = table.Headers.Values
                .Select(header =>
                    PropertyDeclaration(
                            MapType(header.Type),
                            Identifier(header.Name))
                        .WithModifiers(Modifiers.PublicAsList)
                        .WithAccessorList(Accessors.GetSet));

            var rowClassDeclaration = ClassDeclaration(rowName)
                .WithModifiers(Modifiers.PublicAsList)
                .WithMembers(
                    List<MemberDeclarationSyntax>(
                        properties));

            return rowClassDeclaration;
        }

        private static FieldDeclarationSyntax GenerateFields(string rowName)
        {
            var fieldDeclaration = FieldDeclaration(
                VariableDeclaration(
                        GenericName(
                                Identifier("IList"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(rowName)))))
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                Identifier("_value")))))
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                                Token(SyntaxKind.PrivateKeyword),
                                Token(SyntaxKind.StaticKeyword)
                            }));

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
                                    new SyntaxNodeOrToken[]
                                    {
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName("Value"),
                                            LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal(0))),
                                        Token(SyntaxKind.CommaToken),
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName("Result"),
                                            LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal(0))),
                                        Token(SyntaxKind.CommaToken)
                                    }))));

            var declaration = ConstructorDeclaration(Identifier(className))
                .WithModifiers(Modifiers.StaticAsList)
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
                                Identifier("IEnumerable"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(rowName)))),
                        Identifier("Value"))
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