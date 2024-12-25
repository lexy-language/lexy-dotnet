using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Poc.Core.Compiler.Transcribe.ExpressionSyntaxFactory;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    public class FunctionWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node, Nodes nodes)
        {
            if (!(node is Function function))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var name = function.Name.ClassName();

            var members = new List<MemberDeclarationSyntax>();

            members.AddRange(WriteIncludes(function));
            members.AddRange(WriteVariables(function.Parameters.Variables));
            members.AddRange(WriteVariables(function.Results.Variables));

            members.Add(WriteResultMethod(function.Results.Variables));
            members.Add(WriteRunMethod(function));

            var classDeclaration = ClassDeclaration(name)
                .WithModifiers(Modifiers.PublicAsList)
                .WithMembers(List(members));

            return new GeneratedClass(function, name, classDeclaration);
        }

        private IEnumerable<MemberDeclarationSyntax> WriteIncludes(Function function)
        {
            foreach (var include in function.Include.Definitions)
            {
                if (include.Type != IncludeTypes.Table)
                    throw new InvalidOperationException("Invalid include type: " + include.Type);

                var fieldDeclaration = FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName(include.Name))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier(include.Name))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(
                                                    IdentifierName(include.Name))
                                                .WithArgumentList(
                                                    ArgumentList()))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)));

                yield return fieldDeclaration;
            }
        }

        private MemberDeclarationSyntax WriteResultMethod(IList<VariableDefinition> resultVariables)
        {
            var resultType = ParseName($"{typeof(FunctionResult).Namespace}.{nameof(FunctionResult)}");

            var statements = new List<StatementSyntax> {
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                    Identifier("result"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            resultType)
                                            .WithArgumentList(
                                                ArgumentList()))))))
            };

            statements.AddRange(resultVariables.Select(variable =>
                (StatementSyntax) ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        ElementAccessExpression(
                                IdentifierName("result"))
                            .WithArgumentList(
                                BracketedArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(variable.Name)))))),
                        IdentifierName(variable.Name)))));

            statements.Add(ReturnStatement(IdentifierName("result")));

            var function = MethodDeclaration(
                resultType,
                Identifier("__Result"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    Block(statements));

            return function;
        }

        private IEnumerable<MemberDeclarationSyntax> WriteVariables(IList<VariableDefinition> variables)
        {
            foreach (var variable in variables)
            {
                var variableDeclaration = VariableDeclarator(Identifier(variable.Name));
                var defaultValue = TokenValueExpression(variable.Default);
                if (defaultValue != null)
                {
                    variableDeclaration = variableDeclaration.WithInitializer(
                        EqualsValueClause(defaultValue));
                }
                else if (variable.Type is PrimitiveVariableDeclarationType primitiveType)
                {
                    variableDeclaration = variableDeclaration.WithInitializer(
                        EqualsValueClause(
                            PrimitiveTypeDefaultExpression(primitiveType)));
                }

                var fieldDeclaration = FieldDeclaration(
                    VariableDeclaration(MapType(variable))
                        .WithVariables(
                            SingletonSeparatedList(
                                variableDeclaration)))
                    .WithModifiers(Modifiers.PublicAsList);

                yield return fieldDeclaration;
            }
        }

        private MethodDeclarationSyntax WriteRunMethod(Function function)
        {
            var statements = function.Code.Expressions.SelectMany(ExecuteStatementSyntax);

            var functionSyntax = MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("__Run"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(IExecutionContext))))))
                .WithBody(Block(statements));

            return functionSyntax;
        }

    }
}