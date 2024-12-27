using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Types;
using Lexy.RunTime.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Compiler.CSharp.ExpressionSyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp
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
            members.Add(TranslateVariablesClass(LexyCodeConstants.ParametersType, function.Parameters.Variables));
            members.Add(TranslateVariablesClass( LexyCodeConstants.ResultsType, function.Results.Variables));

            members.Add(RunMethod(function, context));

            members.AddRange(CustomBuiltInFunctions(context));

            var name = context.FunctionClassName();

            var classDeclaration = ClassDeclaration(name)
                .WithModifiers(Modifiers.PublicStatic())
                .WithMembers(List(members));

            return new GeneratedClass(function, name, classDeclaration);
        }

        private IEnumerable<MemberDeclarationSyntax> CustomBuiltInFunctions(ICompileFunctionContext context)
        {
            return context.BuiltInFunctionCalls
                .Select(functionCall => functionCall.CustomMethodSyntax(context))
                .Where(customMethodSyntax => customMethodSyntax != null);
        }

        private IEnumerable<FunctionCall> GetBuiltInFunctionCalls(Function function)
        {
            return NodesWalker.WalkWithResult(function.Code.Expressions,
                node => node is FunctionCallExpression expression ? FunctionCall.Create(expression) : null);
        }

        private MemberDeclarationSyntax TranslateVariablesClass(string className, IList<VariableDefinition> variables)
        {
            var fields = TranslateVariablesClass(variables);
            return ClassDeclaration(className)
                .WithModifiers(Modifiers.Public())
                .WithMembers(List(fields));
        }

        private IEnumerable<MemberDeclarationSyntax> TranslateVariablesClass(IList<VariableDefinition> variables)
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
            var statements = new List<StatementSyntax>
            {
                GuardStatements.VerifyNotNull(LexyCodeConstants.ParameterVariable),
                GuardStatements.VerifyNotNull(LexyCodeConstants.ContextVariable),
                InitializeResults(),
            };
            statements.AddRange(function.Code.Expressions.SelectMany(expression => ExecuteStatementSyntax(expression, compileFunctionContext)));
            statements.Add(ReturnResults());

            var functionSyntax = MethodDeclaration(
                    IdentifierName(LexyCodeConstants.ResultsType),
                    Identifier(LexyCodeConstants.RunMethod))
                .WithModifiers(Modifiers.PublicStatic())
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]{
                                Parameter(Identifier(LexyCodeConstants.ParameterVariable))
                                    .WithType(IdentifierName(LexyCodeConstants.ParametersType)),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier(LexyCodeConstants.ContextVariable))
                                    .WithType(IdentifierName(nameof(IExecutionContext)))
                            })))
                .WithBody(Block(statements));

            return functionSyntax;
        }

        private StatementSyntax ReturnResults() => ReturnStatement(IdentifierName(LexyCodeConstants.ResultsVariable));

        private StatementSyntax InitializeResults()
        {
            return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName(
                            Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                    Identifier(LexyCodeConstants.ResultsVariable))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                                IdentifierName(LexyCodeConstants.ResultsType))
                                            .WithArgumentList(
                                                ArgumentList()))))));
        }
    }
}