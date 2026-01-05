using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

//Syntax: "extract(params)"
internal static class ExtractFunctionStatement
{
    public static bool Matches(ExtractResultsFunctionExpression expression) => true;

    public static IEnumerable<StatementSyntax> Create(ExtractResultsFunctionExpression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        return ExtractStatementSyntax(expression.Mapping, expression.FunctionResultVariable);
    }

    public static IEnumerable<StatementSyntax> ExtractStatementSyntax(IEnumerable<Mapping> mappings,
        string functionResultVariable)
    {
        if (mappings == null) throw new ArgumentNullException(nameof(mappings));

        foreach (var mapping in mappings)
        {
            yield return StatementSyntax(functionResultVariable, mapping);
        }
    }

    private static StatementSyntax StatementSyntax(string functionResultVariable, Mapping mapping)
    {
        var left = mapping.VariableSource == VariableSource.Code
            ? IdentifierName(mapping.VariableName)
            : mapping.VariableSource == VariableSource.Results
                ? (ExpressionSyntax)MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ResultsVariable),
                    IdentifierName(mapping.VariableName))
                : throw new InvalidOperationException($"Invalid source: {mapping.VariableSource}");

        var right = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName(functionResultVariable),
            IdentifierName(mapping.VariableName));

        return ExpressionStatement(
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
    }
}