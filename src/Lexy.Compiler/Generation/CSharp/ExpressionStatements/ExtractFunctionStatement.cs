using System.Collections.Generic;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Generation.CSharp.ExpressionStatements;

//Syntax: "extract(params)"
internal static class ExtractFunctionStatement
{
    public static bool Matches(ExtractResultsFunctionExpression expression) => true;

    public static IEnumerable<StatementSyntax> Create(ExtractResultsFunctionExpression expression)
    {
        Assert.NotNull(expression, nameof(expression));

        return ExtractStatementSyntax(expression.StateRequired.Mapping, expression.FunctionResultVariable);
    }

    public static IEnumerable<StatementSyntax> ExtractStatementSyntax(VariablesMapping mappings,
        string functionResultVariable)
    {
        Assert.NotNull(mappings, nameof(mappings));

        foreach (var mapping in mappings)
        {
            yield return StatementSyntax(functionResultVariable, mapping);
        }
    }

    private static StatementSyntax StatementSyntax(string functionResultVariable, Mapping mapping)
    {
        var left = VariableMapping.VariableSyntax(mapping);

        var right = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName(functionResultVariable),
            IdentifierName(mapping.VariableName));

        return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
    }
}
