using System;
using System.Collections.Generic;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatementExceptions;

internal class SimpleLexyFunctionFunctionExpressionStatementException : IExpressionStatementException
{
    public bool Matches(Expression expression)
    {
        return expression is FunctionCallExpression { ExpressionFunction: LexyFunction };
    }

    public IEnumerable<StatementSyntax> CallExpressionSyntax(Expression expression)
    {
        if (!(expression is FunctionCallExpression functionCallExpression))
            throw new InvalidOperationException("expression should be FunctionCallExpression");
        if (!(functionCallExpression.ExpressionFunction is LexyFunction lexyFunction))
            throw new InvalidOperationException(
                "functionCallExpression.ExpressionFunction should be ExtractResultsFunction");

        var parameterVariable = $"{LexyCodeConstants.ParameterVariable}_{expression.Reference.LineNumber}";
        var resultsVariable = $"{LexyCodeConstants.ResultsVariable}_{expression.Reference.LineNumber}";

        var result = new List<StatementSyntax>();
        result.AddRange(FillFunctionExpressionStatementException.FillStatementSyntax(
            parameterVariable,
            lexyFunction.FunctionParametersType,
            lexyFunction.MappingParameters));

        result.Add(RunFunction(lexyFunction, parameterVariable, resultsVariable));

        result.AddRange(ExtractFunctionExpressionStatementException.ExtractStatementSyntax(
            lexyFunction.MappingResults,
            resultsVariable));

        return result;
    }

    private static StatementSyntax RunFunction(LexyFunction lexyFunction, string parameterVariable,
        string resultsVariable)
    {
        var initialize = LexyFunctionCall.RunFunction(lexyFunction.FunctionName, parameterVariable);

        var variable = VariableDeclarator(Identifier(resultsVariable))
            .WithInitializer(EqualsValueClause(initialize));

        return LocalDeclarationStatement(
            VariableDeclaration(Types.Syntax(lexyFunction.FunctionResultsType))
                .WithVariables(SingletonSeparatedList(variable)));
    }
}