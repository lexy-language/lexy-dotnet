using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Compiler.CSharp.ExpressionSyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

public class FunctionWriter : IRootTokenWriter
{
    public GeneratedClass CreateCode(IRootNode node)
    {
        if (node is not Function function) throw new InvalidOperationException("Root token not Function");

        var members = new List<MemberDeclarationSyntax>
        {
            VariableClassFactory.TranslateVariablesClass(LexyCodeConstants.ParametersType, function.Parameters.Variables),
            VariableClassFactory.TranslateVariablesClass(LexyCodeConstants.ResultsType, function.Results.Variables),
            RunMethod(function)
        };
        members.AddRange(CustomBuiltInFunctions(function));

        var name = ClassNames.FunctionClassName(node.NodeName);

        var classDeclaration = ClassDeclaration(name)
            .WithModifiers(Modifiers.PublicStatic())
            .WithMembers(List(members));

        return new GeneratedClass(function, name, classDeclaration);
    }

    private IEnumerable<MemberDeclarationSyntax> CustomBuiltInFunctions(Function function)
    {
        return NodesWalker.WalkWithResult(function.Code.Expressions,
            node => node is FunctionCallExpression expression ? FunctionCallFactory.CustomMethods(expression.ExpressionFunction) : null)
            .Where(value => value != null);
    }

    private MethodDeclarationSyntax RunMethod(Function function)
    {
        var statements = new List<StatementSyntax>
        {
            GuardStatements.VerifyNotNull(LexyCodeConstants.ParameterVariable),
            GuardStatements.VerifyNotNull(LexyCodeConstants.ContextVariable),
            InitializeResults()
        };
        statements.AddRange(function.Code.Expressions.SelectMany(ExecuteStatementSyntax));
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

    private StatementSyntax ReturnResults()
    {
        return ReturnStatement(IdentifierName(LexyCodeConstants.ResultsVariable));
    }

    private StatementSyntax InitializeResults()
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