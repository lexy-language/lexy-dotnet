using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

//var xxx = new(yyy)
internal class NewFunctionStatementCreator : IExpressionStatementCreator<VariableDeclarationExpression>
{
    public bool Matches(VariableDeclarationExpression expression)
    {
        return expression.Assignment is NewFunction;
    }

    public IEnumerable<StatementSyntax> CreateExpressionSyntax(VariableDeclarationExpression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        if (expression.Assignment is not FunctionCallExpression functionCallExpression)
        {
            throw new InvalidOperationException("assignmentExpression.Assignment should be FunctionCallExpression");
        }
        if (functionCallExpression is not NewFunction)
        {
            throw new InvalidOperationException("functionCallExpression.FunctionCallExpression should be NewFunction");
        }

        var typeSyntax = Types.Syntax(expression.Type);

        var initialize = ObjectCreationExpression(typeSyntax)
            .WithArgumentList(ArgumentList());

        var variable = VariableDeclarator(Identifier(expression.Name))
            .WithInitializer(EqualsValueClause(initialize));

        yield return LocalDeclarationStatement(
            VariableDeclaration(typeSyntax)
                .WithVariables(SingletonSeparatedList(variable)));
    }
}