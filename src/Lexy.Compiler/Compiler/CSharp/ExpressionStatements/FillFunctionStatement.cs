using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.VariableTypes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

//Syntax: "var result = fill(params)"
internal static class FillFunctionStatement
{
    public static bool Matches(VariableDeclarationExpression expression)
    {
        return expression.Assignment is FillParametersFunctionExpression;
    }

    public static IEnumerable<StatementSyntax> Create(VariableDeclarationExpression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        if (expression.Assignment is not FunctionCallExpression functionCallExpression)
        {
            throw new InvalidOperationException("assignmentExpression.Assignment should be FunctionCallExpression");
        }
        if (functionCallExpression is not FillParametersFunctionExpression fillParametersFunction)
        {
            throw new InvalidOperationException(
                "functionCallExpression.FunctionCallExpression should be FillParametersFunction");
        }

        return FillStatementSyntax(expression.Name, fillParametersFunction.Type, fillParametersFunction.Mapping);
    }

    public static IEnumerable<StatementSyntax> FillStatementSyntax(string variableName, VariableType type,
        IEnumerable<Mapping> mappings)
    {
        if (variableName == null) throw new ArgumentNullException(nameof(variableName));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (mappings == null) throw new ArgumentNullException(nameof(mappings));

        var typeSyntax = Types.Syntax(type);

        var initialize = ObjectCreationExpression(Types.Syntax(type))
            .WithArgumentList(ArgumentList());

        var variable = VariableDeclarator(Identifier(variableName))
            .WithInitializer(EqualsValueClause(initialize));

        yield return LocalDeclarationStatement(
            VariableDeclaration(typeSyntax)
                .WithVariables(SingletonSeparatedList(variable)));

        foreach (var mapping in mappings)
        {
            var left = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(variableName),
                IdentifierName(mapping.VariableName));

            var right = mapping.VariableSource == VariableSource.Code
                ? IdentifierName(mapping.VariableName)
                : mapping.VariableSource == VariableSource.Parameters
                    ? (ExpressionSyntax)MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ParameterVariable),
                        IdentifierName(mapping.VariableName))
                    : throw new InvalidOperationException("Invalid source: " + mapping.VariableSource);

            yield return ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
        }
    }
}