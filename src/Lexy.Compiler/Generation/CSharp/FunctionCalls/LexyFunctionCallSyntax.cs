using System.Collections.Generic;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Generation.CSharp.FunctionCalls;

//LexyFunction(variable)
internal static class LexyFunctionCallSyntax
{
    public static bool Matches(LexyFunctionCallExpression expression)
    {
        return true;
    }

    public static ExpressionSyntax Create(LexyFunctionCallExpression expression)
    {
        return RunFunction(expression.FunctionName, expression.Arguments, expression.ParametersMapping);
    }

    public static InvocationExpressionSyntax RunFunction(string functionName, string variableName)
    {
        var argumentsSyntax = new SyntaxNodeOrToken[]
        {
            Argument(IdentifierName(variableName)),
            Token(SyntaxKind.CommaToken),
            Argument(IdentifierName(LexyCodeConstants.ContextVariable))
        };

        return InvocationExpressionSyntax(LexyCodeConstants.RunMethod, functionName, argumentsSyntax);
    }

    private static InvocationExpressionSyntax RunFunction(string functionName, IReadOnlyList<Expression> arguments,
        VariablesMapping mapping)
    {
        var argumentsSyntax = GetArguments(arguments, mapping);
        argumentsSyntax.Add(Token(SyntaxKind.CommaToken));
        argumentsSyntax.Add(Argument(IdentifierName(LexyCodeConstants.ContextVariable)));

        return InvocationExpressionSyntax(LexyCodeConstants.RunMethod, functionName, argumentsSyntax);
    }

    private static InvocationExpressionSyntax InvocationExpressionSyntax(string runMethodName, string functionName, IReadOnlyList<SyntaxNodeOrToken> argumentsSyntax)
    {
        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(ClassNames.FunctionClassName(functionName)),
                    IdentifierName(runMethodName)))
            .WithArgumentList(
                ArgumentList(
                    SeparatedList<ArgumentSyntax>(argumentsSyntax)));
    }

    private static List<SyntaxNodeOrToken> GetArguments(IReadOnlyList<Expression> arguments, VariablesMapping mappings)
    {
        if (arguments.Count == 1 && arguments[0] is SpreadExpression)
        {
            return new List<SyntaxNodeOrToken>{MappedParametersObject(mappings)};
        }

        var argumentsSyntax = new List<SyntaxNodeOrToken>();
        foreach (var argument in arguments)
        {
            if (argumentsSyntax.Count > 0)
            {
                argumentsSyntax.Add(Token(SyntaxKind.CommaToken));
            }

            argumentsSyntax.Add(Argument(Expressions.ExpressionSyntax(argument)));
        }
        return argumentsSyntax;
    }

    private static ArgumentSyntax MappedParametersObject(VariablesMapping mappings)
    {
        Assert.NotNull(mappings, "expression.mapping");

        var mappingsSyntax = new List<SyntaxNodeOrToken>();

        for (var index = 0; index < mappings.Count; index++)
        {
            var mapping = mappings[index];
            mappingsSyntax.Add(VariableMapping.AssignmentSyntax(mapping));

            if (index < mappings.Count - 1)
            {
                mappingsSyntax.Add(Token(SyntaxKind.CommaToken));
            }
        }

        return Argument(
            ObjectCreationExpression(Types.Syntax(mappings.MappingType))
            .WithInitializer(
                InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        mappingsSyntax))));
    }

}