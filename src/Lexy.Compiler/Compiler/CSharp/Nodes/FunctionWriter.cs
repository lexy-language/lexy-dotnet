using System;
using System.Collections.Generic;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Compiler.CSharp.Syntax.Expressions;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

public static class FunctionWriter
{
    public static GeneratedClass CreateCode(Function function)
    {
        if (function == null) throw new ArgumentNullException(nameof(function));

        var members = new List<MemberDeclarationSyntax>
        {
            VariableClass.TranslateVariablesClass(LexyCodeConstants.ParametersType, function.Parameters.Variables),
            VariableClass.TranslateVariablesClass(LexyCodeConstants.ResultsType, function.Results.Variables),
            RunMethod(function)
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