using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

internal static class LogCalls
{
    public static ExpressionStatementSyntax SetFileName(string fileName)
    {
        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ContextVariable),
                        IdentifierName(nameof(IExecutionContext.SetFileName))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(fileName)))))));
    }

    public static ExpressionStatementSyntax OpenScope(string scopeName, int lineNumber)
    {
        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ContextVariable),
                        IdentifierName(nameof(IExecutionContext.OpenScope))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Arguments.String(scopeName),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(lineNumber)
                            }))));
    }

    public static ExpressionStatementSyntax CloseScope()
    {
        return CallContextMethod(nameof(IExecutionContext.CloseScope));
    }

    public static LocalDeclarationStatementSyntax LogLine(int lineNumber, string message, IEnumerable<VariableUsage> variables)
    {
        var buildExpression =
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ContextVariable),
                        IdentifierName(nameof(IExecutionContext.LogLine))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new []
                            {
                                Arguments.String(message),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(lineNumber),
                                Token(SyntaxKind.CommaToken),
                                (SyntaxNodeOrToken) Argument(BuildVariables(variables.ToArray()))
                            })));

        return LocalDeclarationStatement(
            VariableDeclaration(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            TriviaList())))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                                Identifier(LineVariableName(lineNumber)))
                            .WithInitializer(EqualsValueClause(buildExpression)))));
    }

    public static ExpressionStatementSyntax LogVariables(int? lineNumber, string message, string variableName)
    {
        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ContextVariable),
                        IdentifierName(nameof(IExecutionContext.LogVariables))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Arguments.String(message),
                                Token(SyntaxKind.CommaToken),
                                lineNumber != null ? Arguments.Numeric(lineNumber.Value) : Arguments.Null(),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName(variableName))
                            }))));
    }

    public static ExpressionStatementSyntax AddWriteVariables(int lineNumber, IEnumerable<VariableUsage> variables)
    {
        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LineVariableName(lineNumber)),
                        IdentifierName(nameof(ExecutionLogEntry.AddWriteVariables))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                BuildVariables(variables.ToArray()))))));
    }

    private static string LineVariableName(int lineName) => "__logLine" + lineName;

    private static InvocationExpressionSyntax BuildVariables(VariableUsage[] variables)
    {
        var newBuilder = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(nameof(LogVariablesBuilder)),
                IdentifierName(nameof(LogVariablesBuilder.New))));

        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                BuildVariables(variables, newBuilder),
                IdentifierName("Build")));
    }

    private static ExpressionSyntax BuildVariables(VariableUsage[] variables, ExpressionSyntax newStatement)
    {
        var variable = variables?.LastOrDefault();
        if (variable == null) return newStatement;

        var rest = variables.Length > 1 ? variables.Take(variables.Length - 1).ToArray() : null;
        var variableExpression = variable.Source == VariableSource.Type
            ? LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(variable.Path.FullPath()))
            : VariableReferences.Translate(variable);
        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    BuildVariables(rest, newStatement),
                    IdentifierName(nameof(LogVariablesBuilder.AddVariable))))
            .WithArgumentList(
                ArgumentList(
                    SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Arguments.String(variable.Path.ToString()),
                            Token(SyntaxKind.CommaToken),
                            Argument(variableExpression)
                        })));
    }

    public static ExpressionStatementSyntax UseLastNodeAsScope()
    {
        return CallContextMethod(nameof(IExecutionContext.UseLastNodeAsScope));
    }

    public static ExpressionStatementSyntax RevertToParentScope()
    {
        return CallContextMethod(nameof(IExecutionContext.RevertToParentScope));
    }

    private static ExpressionStatementSyntax CallContextMethod(string methodName)
    {
        return ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ContextVariable),
                    IdentifierName(methodName))));
    }


    public static StatementSyntax LogLineAndVariables(Expression expression)
    {
        var variables = GetReadVariables(expression);
        var codeLine = expression.Source.Line.Content.Substring(2);
        return LogLine(expression.Source.Line.Index, codeLine, variables);
    }

    public static StatementSyntax LogAssignmentVariables(Expression expression)
    {
        var variables = GetWriteVariables(expression);
        return AddWriteVariables(expression.Source.Line.Index, variables);
    }

    private static IEnumerable<VariableUsage> GetUsedVariables(Expression expression, VariableAccess access) {
      var usage = expression.UsedVariables();
      return usage.Where(variable => variable.Access == access)
          .DistinctBy(variable => variable.Path.FullPath())
          .ToList();
    }

    private static IEnumerable<VariableUsage> GetReadVariables(Expression expression) {
      return GetUsedVariables(expression, VariableAccess.Read);
    }

    private static IEnumerable<VariableUsage> GetWriteVariables(Expression expression) {
      return GetUsedVariables(expression, VariableAccess.Write);
    }
}