using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Generation.CSharp.Syntax.Expressions;

namespace Lexy.Compiler.Generation.CSharp.Components;

public static class FunctionClass
{
    public static GeneratedClass CreateCode(Function function)
    {
        if (function == null) throw new ArgumentNullException(nameof(function));

        var members = new List<MemberDeclarationSyntax>
        {
            VariableClass.Syntax(LexyCodeConstants.ParametersType, function.Parameters.Variables),
            VariableClass.Syntax(LexyCodeConstants.ResultsType, function.Results.Variables),
            RunMethod(function),
            RunMethodInlineArguments(function)
        };

        var name = ClassNames.FunctionClassName(function.NodeName);

        var classDeclaration = ClassDeclaration(name)
            .WithModifiers(Modifiers.PublicStatic())
            .WithMembers(List(members));

        return new GeneratedClass(function, name, classDeclaration);
    }

    private static MethodDeclarationSyntax RunMethod(Function function)
    {
        var statements = new List<StatementSyntax>
        {
            GuardStatements.VerifyNotNull(LexyCodeConstants.ParameterVariable),
            GuardStatements.VerifyNotNull(LexyCodeConstants.ContextVariable),
            LogCalls.SetFileName(function.Reference.File.FileName),
            LogCalls.OpenScope($"Execute: {function.NodeName}", function.Reference.LineNumber ?? -1),
        };

        if (function.Parameters != null)
        {
            statements.Add(LogCalls.LogVariables(function.Parameters.Reference?.LineNumber, "Parameters",
                LexyCodeConstants.ParameterVariable));
        }

        statements.Add(InitializeResults());

        statements.AddRange(ExecuteExpressionStatementSyntax(function.Code.Expressions, false));
        if (function.Results != null)
        {
            statements.Add(LogCalls.LogVariables(function.Results.Reference.LineNumber, "Results",
                LexyCodeConstants.ResultsVariable));
        }

        statements.Add(LogCalls.CloseScope());
        statements.Add(ReturnResults());

        var functionSyntax = MethodDeclaration(
                IdentifierName(LexyCodeConstants.ResultsType),
                Identifier(LexyCodeConstants.RunMethod))
            .WithModifiers(Modifiers.PublicStatic())
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(Identifier(LexyCodeConstants.ParameterVariable))
                                .WithType(IdentifierName(LexyCodeConstants.ParametersType)),
                            Token(SyntaxKind.CommaToken),
                            Parameter(Identifier(LexyCodeConstants.ContextVariable))
                                .WithType(IdentifierName(nameof(IExecutionContext)))
                        })))
            .WithBody(Block(statements));

        return functionSyntax;
    }

    private static MethodDeclarationSyntax RunMethodInlineArguments(Function function)
    {
        var statements = new List<StatementSyntax>
        {
            LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName(
                            Identifier(TriviaList(), SyntaxKind.VarKeyword,"var", "var",TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(Identifier(LexyCodeConstants.ParameterVariable))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(IdentifierName(LexyCodeConstants.ParametersType))
                                            .WithArgumentList(
                                                ArgumentList()))))))
        };

        statements.AddRange(function.Parameters.Variables
            .Select(variable => ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ParameterVariable),
                        IdentifierName(variable.Name)),
                    IdentifierName(variable.Name)))));

        statements.Add(
            ReturnStatement(
                InvocationExpression(IdentifierName(LexyCodeConstants.RunMethod))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(IdentifierName(LexyCodeConstants.ParameterVariable)),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(IdentifierName(LexyCodeConstants.ContextVariable))
                                })))));

        var parameters = new List<SyntaxNodeOrToken>();

        foreach (var variable in function.Parameters.Variables)
        {
            parameters.Add(Parameter(Identifier(variable.Name))
                    .WithType(Types.Syntax(variable.VariableType)));
            parameters.Add(Token(SyntaxKind.CommaToken));
        }

        parameters.Add(Parameter(Identifier(LexyCodeConstants.ContextVariable))
            .WithType(IdentifierName(nameof(IExecutionContext))));

        var functionSyntax = MethodDeclaration(
                IdentifierName(LexyCodeConstants.ResultsType),
                Identifier(LexyCodeConstants.RunMethod))
            .WithModifiers(Modifiers.PublicStatic())
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(parameters)))
            .WithBody(Block(statements));

        return functionSyntax;
    }

    private static StatementSyntax ReturnResults()
    {
        return ReturnStatement(IdentifierName(LexyCodeConstants.ResultsVariable));
    }

    private static StatementSyntax InitializeResults()
    {
        return LocalDeclarationStatement(
            VariableDeclaration(
                    IdentifierName(
                        Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                .WithVariables(
                    SingletonSeparatedList(
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