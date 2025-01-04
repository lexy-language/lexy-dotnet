using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

public static class VariableClassFactory
{
    public static MemberDeclarationSyntax TranslateVariablesClass(string className, IReadOnlyList<VariableDefinition> variables)
    {
        var fields = TranslateVariablesClass(variables);
        return SyntaxFactory.ClassDeclaration(className)
            .WithModifiers(Modifiers.Public())
            .WithMembers(SyntaxFactory.List(fields));
    }

    private static IEnumerable<MemberDeclarationSyntax> TranslateVariablesClass(IReadOnlyList<VariableDefinition> variables)
    {
        return variables.Select(TranslateVariable);
    }

    private static FieldDeclarationSyntax TranslateVariable(VariableDefinition variable)
    {
        var initializer = DefaultExpression(variable);

        var variableDeclaration = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variable.Name))
            .WithInitializer(SyntaxFactory.EqualsValueClause(initializer));


        var fieldDeclaration = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(Types.Syntax(variable))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclaration)))
            .WithModifiers(Modifiers.Public());
        return fieldDeclaration;
    }

    private static ExpressionSyntax DefaultExpression(VariableDefinition variable)
    {
        var defaultValue = variable.DefaultExpression != null
            ? ExpressionSyntaxFactory.ExpressionSyntax(variable.DefaultExpression)
            : null;
        return defaultValue ?? Types.TypeDefaultExpression(variable.Type);
    }
}