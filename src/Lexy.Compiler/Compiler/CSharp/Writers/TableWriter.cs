using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Tables;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

internal class TableWriter : IComponentTokenWriter
{
    public GeneratedClass CreateCode(IComponentNode node)
    {
        if (!(node is Table table)) throw new InvalidOperationException("Component token not table");

        var className = ClassNames.TableClassName(table.Name.Value);

        var members = new List<MemberDeclarationSyntax>();
        members.Add(GenerateRowClass(LexyCodeConstants.RowType, table));
        members.Add(GenerateFields(LexyCodeConstants.RowType));
        members.Add(GenerateStaticConstructor(className, table, LexyCodeConstants.RowType));
        members.AddRange(GenerateProperties(LexyCodeConstants.RowType));

        var classDeclaration = ClassDeclaration(className)
            .WithModifiers(Modifiers.Public())
            .WithMembers(List(members));

        return new GeneratedClass(node, className, classDeclaration);
    }

    private static ClassDeclarationSyntax GenerateRowClass(string rowName, Table table)
    {
        var fields = List<MemberDeclarationSyntax>(
            table.Header.Columns
                .Select(Field));

        var rowClassDeclaration = ClassDeclaration(rowName)
            .WithModifiers(Modifiers.Public())
            .WithMembers(fields);

        return rowClassDeclaration;
    }

    private static FieldDeclarationSyntax Field(ColumnHeader header)
    {
        return FieldDeclaration(
                VariableDeclaration(Types.Syntax(header.Type))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(header.Name))
                        .WithInitializer(EqualsValueClause(
                            Types.TypeDefaultExpression(header.Type))))))
            .WithModifiers(Modifiers.Public());
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
                            VariableDeclarator(Identifier(LexyCodeConstants.ValuesVariable)))))
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
                                IdentifierName(LexyCodeConstants.ValuesVariable),
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
        for (var index = 0; index < header.Columns.Count; index++)
        {
            var columnHeader = header.Columns[index];
            var value = tableRow.Values[index].Expression;

            if (result.Count > 0) result.Add(Token(SyntaxKind.CommaToken));

            result.Add(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(columnHeader.Name),
                    Expressions.ExpressionSyntax(value)));
        }

        return result.ToArray();
    }

    private static IEnumerable<PropertyDeclarationSyntax> GenerateProperties(string rowName)
    {
        yield return PropertyDeclaration(
                PredefinedType(
                    Token(SyntaxKind.IntKeyword)),
                Identifier("Count"))
            .WithModifiers(Modifiers.PublicStatic())
            .WithExpressionBody(
                ArrowExpressionClause(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ValuesVariable),
                        IdentifierName("Count"))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));

        yield return
            PropertyDeclaration(
                    GenericName(Identifier("IReadOnlyList"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(rowName)))),
                    Identifier("Values"))
                .WithModifiers(Modifiers.PublicStatic())
                .WithExpressionBody(
                    ArrowExpressionClause(
                        IdentifierName(LexyCodeConstants.ValuesVariable)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
    }
}