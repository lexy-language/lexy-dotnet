using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

public static class VariableClass
{
    public static MemberDeclarationSyntax Syntax(string className, IReadOnlyList<VariableDefinition> variables)
    {
        var fields = Syntax(variables);
        return SyntaxFactory.ClassDeclaration(className)
            .WithModifiers(Modifiers.Public())
            .WithMembers(SyntaxFactory.List(fields));
    }

    private static IEnumerable<MemberDeclarationSyntax> Syntax(IReadOnlyList<VariableDefinition> variables)
    {
        return variables.Select(VariableSyntax);
    }

    private static FieldDeclarationSyntax VariableSyntax(VariableDefinition variable)
    {
        var initializer = DefaultExpression(variable);

        var variableDeclaration = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variable.Name))
            .WithInitializer(SyntaxFactory.EqualsValueClause(initializer));

        var typeSyntax = Types.Syntax(variable);
        var fieldDeclaration = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(typeSyntax)
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclaration)))
            .WithModifiers(Modifiers.Public());
        return fieldDeclaration;
    }

    private static ExpressionSyntax DefaultExpression(VariableDefinition variable)
    {
        var defaultValue = variable.DefaultExpression != null
            ? Expressions.ExpressionSyntax(variable.DefaultExpression)
            : null;
        return defaultValue ?? Types.TypeDefaultExpression(variable.Type);
    }
}