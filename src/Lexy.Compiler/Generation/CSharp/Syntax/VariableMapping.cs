using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Generation.CSharp.Syntax;

internal static class VariableMapping
{
    internal static ExpressionSyntax AssignmentSyntax(Mapping mapping)
    {
        var left = IdentifierName(mapping.VariableName);
        var right = VariableSyntax(mapping);

        return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
    }

    internal static ExpressionSyntax VariableSyntax(Mapping mapping)
    {
        if (mapping.VariableSource == VariableSource.Code)
        {
            return IdentifierName(mapping.VariableName);
        }

        if (mapping.VariableSource == VariableSource.Parameters)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(LexyCodeConstants.ParameterVariable),
                IdentifierName(mapping.VariableName));
        }

        if (mapping.VariableSource == VariableSource.Results)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(LexyCodeConstants.ResultsVariable),
                IdentifierName(mapping.VariableName));
        }

        throw new InvalidOperationException("Invalid source: " + mapping.VariableSource);
    }
}