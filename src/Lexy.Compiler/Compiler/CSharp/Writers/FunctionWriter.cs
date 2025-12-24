using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Compiler.CSharp.Syntax.Expressions;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

public class FunctionWriter : IComponentTokenWriter
{
    public GeneratedClass CreateCode(IComponentNode node)
    {
        if (node is not Function function) throw new InvalidOperationException("Component token not Function");

        var members = new List<MemberDeclarationSyntax>
        {
            VariableClass.TranslateVariablesClass(LexyCodeConstants.ParametersType, function.Parameters.Variables),
            VariableClass.TranslateVariablesClass(LexyCodeConstants.ResultsType, function.Results.Variables),
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
            node => node is FunctionCallExpression expression ? FunctionCallFactory.CustomMethods(expression) : null)
            .Where(value => value != null);
    }

    private MethodDeclarationSyntax RunMethod(Function function)
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

    private IEnumerable<VariableUsage> Variables(ComplexType complexType, IReadOnlyList<VariableDefinition> variables, VariableSource source)
    {
        return variables.Select(variable => new VariableUsage(
            VariablePathParser.Parse(variable.Name),
            complexType,
            variable.VariableType,
            source,
            VariableAccess.Read));
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