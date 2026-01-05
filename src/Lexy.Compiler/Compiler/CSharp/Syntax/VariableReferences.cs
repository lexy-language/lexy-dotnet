using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

internal static class VariableReferences
{
    public static ExpressionSyntax Syntax(VariableReference variableReference)
    {
        if (variableReference == null) throw new ArgumentNullException(nameof(variableReference));

        var parentIdentifier = ParentVariableClassName(variableReference);
        var parent = FromSource(variableReference.Source, parentIdentifier);

        ExpressionSyntax result = null;

        var childReference = variableReference.Path;
        while (childReference.HasChildIdentifiers)
        {
            childReference = childReference.ChildrenReference();
            result = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                result ?? parent,
                IdentifierName(childReference.RootIdentifier));
        }

        return result ?? parent;
    }

    private static ExpressionSyntax FromSource(VariableSource source, string name)
    {
        switch (source)
        {
            case VariableSource.Parameters:
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ParameterVariable),
                    IdentifierName(name));

            case VariableSource.Results:
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ResultsVariable),
                    IdentifierName(name));

            case VariableSource.Code:
            case VariableSource.Type:
                return IdentifierName(name);

            case VariableSource.Unknown:
            default:
                throw new ArgumentOutOfRangeException($"source: {source}");
        }
    }

    private static string ParentVariableClassName(VariableReference reference)
    {
        var parentIdentifier = reference.Path.RootIdentifier;
        return reference.ComponentType switch
        {
            TableType _ => ClassNames.TableClassName(parentIdentifier),
            FunctionType _ => ClassNames.FunctionClassName(parentIdentifier),
            EnumType _ => ClassNames.EnumClassName(parentIdentifier),
            DeclaredType _ => ClassNames.TypeClassName(parentIdentifier),
            _ => parentIdentifier
        };
    }
}