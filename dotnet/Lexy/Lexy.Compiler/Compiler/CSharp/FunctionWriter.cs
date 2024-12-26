using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Poc.Core.Compiler.CSharp.ExpressionSyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    public class FunctionWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node)
        {
            if (!(node is Function function))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var builtInFunctionCalls = GetBuiltInFunctionCalls(function);
            var context = new CompileFunctionContext(function, builtInFunctionCalls);

            var members = new List<MemberDeclarationSyntax>();
            members.AddRange(TranslateIncludes(function));
            members.AddRange(TranslateVariables(function.Parameters.Variables));
            members.AddRange(TranslateVariables(function.Results.Variables));

            members.AddRange(CustomBuiltInFunctions(context));

            members.Add(ResultMethod(function.Results.Variables));
            members.Add(RunMethod(function, context));

            var name = context.ClassName();
            var classDeclaration = ClassDeclaration(name)
                .WithModifiers(Modifiers.Public())
                .WithMembers(List(members));

            return new GeneratedClass(function, name, classDeclaration);
        }

        private IEnumerable<MemberDeclarationSyntax> CustomBuiltInFunctions(ICompileFunctionContext context)
        {
            return context.BuiltInFunctionCalls
                .Select(functionCall => functionCall.CustomMethodSyntax(context))
                .Where(customMethodSyntax => customMethodSyntax != null);
        }

        private IEnumerable<BuiltInFunctionCall> GetBuiltInFunctionCalls(Function function)
        {
            return NodesWalker.Walk(function.Code.Expressions,
                node => node is FunctionCallExpression expression ? BuiltInFunctionCall.Create(expression) : null);
        }

        private IEnumerable<MemberDeclarationSyntax> TranslateIncludes(Function function)
        {
            foreach (var include in function.Include.Definitions)
            {
                if (include.Type != IncludeTypes.Table)
                    throw new InvalidOperationException("Invalid include type: " + include.Type);

                var fieldDeclaration = FieldDeclaration(
                    VariableDeclaration(IdentifierName(include.Name))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier(include.Name))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(IdentifierName(include.Name))
                                                .WithArgumentList(ArgumentList()))))))
                    .WithModifiers(Modifiers.Public());

                yield return fieldDeclaration;
            }
        }

        private static MemberDeclarationSyntax ResultMethod(IList<VariableDefinition> resultVariables)
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
                Identifier(LexyCodeConstants.ResultMethod))
                .WithModifiers(Modifiers.Public())
                .WithBody(
                    Block(statements));

            return function;
        }

        private IEnumerable<MemberDeclarationSyntax> TranslateVariables(IList<VariableDefinition> variables)
        {
            foreach (var variable in variables)
            {
                var variableDeclaration = VariableDeclarator(Identifier(variable.Name));
                var defaultValue = TokenValuesSyntax.Expression(variable.Default);
                if (defaultValue != null)
                {
                    variableDeclaration = variableDeclaration.WithInitializer(
                        EqualsValueClause(defaultValue));
                }
                else if (variable.Type is PrimitiveVariableDeclarationType primitiveType)
                {
                    variableDeclaration = variableDeclaration.WithInitializer(
                        EqualsValueClause(
                            Types.PrimitiveTypeDefaultExpression(primitiveType)));
                }

                var fieldDeclaration = FieldDeclaration(
                    VariableDeclaration(Types.Syntax(variable))
                        .WithVariables(
                            SingletonSeparatedList(
                                variableDeclaration)))
                    .WithModifiers(Modifiers.Public());

                yield return fieldDeclaration;
            }
        }

        private MethodDeclarationSyntax RunMethod(Function function,
            ICompileFunctionContext compileFunctionContext)
        {
            var statements = function.Code.Expressions.SelectMany(expression => ExecuteStatementSyntax(expression, compileFunctionContext));

            var functionSyntax = MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier(LexyCodeConstants.RunMethod))
                .WithModifiers(Modifiers.Public())
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(Identifier("context"))
                                .WithType(IdentifierName(nameof(IExecutionContext))))))
                .WithBody(Block(statements));

            return functionSyntax;
        }
    }
}