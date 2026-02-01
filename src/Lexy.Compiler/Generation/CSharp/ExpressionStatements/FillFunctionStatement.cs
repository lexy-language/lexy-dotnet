using System;
using System.Collections.Generic;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Generation.CSharp.ExpressionStatements;

//Syntax: "var result = fill(params)"
internal static class FillFunctionStatement
{
    public static bool Matches(VariableDeclarationExpression expression)
    {
        return expression.Assignment is FillParametersFunctionExpression;
    }

    public static IEnumerable<StatementSyntax> Create(VariableDeclarationExpression expression)
    {
        Assert.NotNull(expression, nameof(expression));

        if (expression.Assignment is not FunctionCallExpression functionCallExpression)
        {
            throw new InvalidOperationException("assignmentExpression.Assignment should be FunctionCallExpression");
        }
        if (functionCallExpression is not FillParametersFunctionExpression fillParametersFunction)
        {
            throw new InvalidOperationException(
                "functionCallExpression.FunctionCallExpression should be FillParametersFunction");
        }

        return FillStatementSyntax(expression.Name, fillParametersFunction.Type, fillParametersFunction.State.Mapping);
    }

    public static IEnumerable<StatementSyntax> FillStatementSyntax(string variableName, Type type,
        IEnumerable<Mapping> mappings)
    {
        Assert.NotNull(variableName, nameof(variableName));
        Assert.NotNull(type, nameof(type));
        Assert.NotNull(mappings, nameof(mappings));

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

            var right = VariableMapping.VariableSyntax(mapping);

            yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
        }
    }
}
