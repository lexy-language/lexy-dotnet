using System;
using System.Collections.Generic;
using Lexy.Compiler.Compiler.CSharp.FunctionCalls;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

//A lexy function is called without assignment (at statement level) which means it should auto map it's results and parameters
//Otherwise LexyFunctionCallExpression is translated by LexyFunctionCall
//LexyFunction()
internal class AutoMapLexyFunctionStatementCreator : IExpressionStatementCreator<LexyFunctionCallExpression>
{
    public bool Matches(LexyFunctionCallExpression expression)
    {
        if (!expression.AutoMap)
        {
            throw new InvalidOperationException("AutoMap should be set to true for a statement.");
        }
        return true;
    }

    public IEnumerable<StatementSyntax> CreateExpressionSyntax(LexyFunctionCallExpression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        var parameterVariable = $"{LexyCodeConstants.ParameterVariable}_{expression.Reference.LineNumber}";
        var resultsVariable = $"{LexyCodeConstants.ResultsVariable}_{expression.Reference.LineNumber}";

        var result = new List<StatementSyntax>();

        result.AddRange(FillFunctionStatementCreator.FillStatementSyntax(
            parameterVariable,
            expression.FunctionParametersTypes,
            expression.MappingParameters));

        result.Add(RunFunction(expression, parameterVariable, resultsVariable));

        result.AddRange(ExtractFunctionStatementCreator.ExtractStatementSyntax(
            expression.MappingResults,
            resultsVariable));

        return result;
    }

    private static StatementSyntax RunFunction(LexyFunctionCallExpression lexyFunctionCallExpression, string parameterVariable,
        string resultsVariable)
    {
        var initialize = LexyFunctionCallCreator.RunFunction(lexyFunctionCallExpression.FunctionName, parameterVariable);

        var variable = VariableDeclarator(Identifier(resultsVariable))
            .WithInitializer(EqualsValueClause(initialize));

        return LocalDeclarationStatement(
            VariableDeclaration(Types.Syntax(lexyFunctionCallExpression.FunctionResultsType))
                .WithVariables(SingletonSeparatedList(variable)));
    }
}