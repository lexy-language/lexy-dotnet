using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp
{
    internal class TypeWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootNode node)
        {
            if (node is not TypeDefinition typeDefinition)
            {
                throw new InvalidOperationException("Root token not table");
            }

            var className = ClassNames.TypeClassName(typeDefinition.Name.Value);

            var classDeclaration = VariableClassFactory.TranslateVariablesClass(className, typeDefinition.Variables);

            return new GeneratedClass(node, className, classDeclaration);
        }
    }

    public static class VariableClassFactory
    {

        public static MemberDeclarationSyntax TranslateVariablesClass(string className, IList<VariableDefinition> variables)
        {
            var fields = TranslateVariablesClass(variables);
            return ClassDeclaration(className)
                .WithModifiers(Modifiers.Public())
                .WithMembers(List(fields));
        }

        private static IEnumerable<MemberDeclarationSyntax> TranslateVariablesClass(IList<VariableDefinition> variables)
        {
            return variables.Select(TranslateVariable);
        }

        private static FieldDeclarationSyntax TranslateVariable(VariableDefinition variable)
        {
            var initializer = DefaultExpression(variable);

            var variableDeclaration = VariableDeclarator(Identifier(variable.Name))
                .WithInitializer(EqualsValueClause(initializer));


            var fieldDeclaration = FieldDeclaration(VariableDeclaration(Types.Syntax(variable))
                    .WithVariables(SingletonSeparatedList(variableDeclaration)))
                .WithModifiers(Modifiers.Public());
            return fieldDeclaration;
        }

        private static ExpressionSyntax DefaultExpression(VariableDefinition variable)
        {
            var defaultValue = variable.DefaultExpression != null ? ExpressionSyntaxFactory.ExpressionSyntax(variable.DefaultExpression) : null;
            return defaultValue ?? Types.TypeDefaultExpression(variable.Type);
        }
    }
}