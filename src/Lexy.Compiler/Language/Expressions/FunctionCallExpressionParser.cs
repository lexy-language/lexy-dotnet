using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public static class FunctionCallExpressionParser
{
    private static readonly IDictionary<string, Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult>>
        SystemFunctions = new Dictionary<string, Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult>>
        {
            { NewFunction.Name, Create(NewFunction.Create) },
            { FillParametersFunction.Name, Create(FillParametersFunction.Create) },
            { ExtractResultsFunction.Name, Create(ExtractResultsFunction.Create) }
        };

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!FunctionCallExpression.IsValid(tokens))
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>("Not valid.");
        }

        var matchingClosingParenthesis = ParenthesizedExpression.FindMatchingClosingParenthesis(tokens);
        if (matchingClosingParenthesis == -1)
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>("No closing parentheses found.");
        }

        var functionNameExpression = tokens[0];
        var argumentsTokenListResult = GetArgumentTokens(source, factory, tokens, matchingClosingParenthesis);
        if (!argumentsTokenListResult.IsSuccess)
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>(argumentsTokenListResult.ErrorMessage);
        }

        var builtInFunctionResult = Parse(functionNameExpression, source, argumentsTokenListResult.Result);
        if (!builtInFunctionResult.IsSuccess)
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>(builtInFunctionResult.ErrorMessage);
        }

        return ParseExpressionResult.Success(builtInFunctionResult?.Result);
    }

    private static ParseExpressionsResult GetArgumentTokens(
        ExpressionSource source, IExpressionFactory factory,
        TokenList tokens, int matchingClosingParenthesis)
    {

        var innerExpressionTokens = tokens.TokensRange(2, matchingClosingParenthesis - 1);
        var argumentsTokenList = ArgumentList.Parse(innerExpressionTokens);

        var arguments = new List<Expression>();
        foreach (var argumentTokens in argumentsTokenList.Result)
        {
            var argumentExpression = factory.Parse(argumentTokens, source.Line);
            if (!argumentExpression.IsSuccess)
            {
                return ParseExpressionsResult.Invalid<FunctionCallExpression>(argumentExpression.ErrorMessage);
            }

            arguments.Add(argumentExpression.Result);
        }

        return ParseExpressionsResult.Success(arguments);
    }

    private static ParseExpressionFunctionsResult Parse(Token functionNameToken, ExpressionSource source,
        IReadOnlyList<Expression> arguments)
    {
        return functionNameToken switch
        {
            StringLiteralToken stringLiteralToken =>
                ParseStringLiteralFunctionCall(source, arguments, stringLiteralToken),

            MemberAccessLiteralToken memberAccessLiteralToken =>
                CreateMemberFunctionCallExpression(source, arguments, memberAccessLiteralToken),

            _ => throw new InvalidOperationException($"Invalid token type: {functionNameToken.GetType()}")
        };
    }

    private static ParseExpressionFunctionsResult ParseStringLiteralFunctionCall(ExpressionSource source,
        IReadOnlyList<Expression> arguments, StringLiteralToken stringLiteralToken)
    {
        var functionName = stringLiteralToken.Value;
        return SystemFunctions.TryGetValue(functionName, out var value)
            ? value(source, arguments)
            : ParseExpressionFunctionsResult.Success(CreateLexyFunctionCallExpression(functionName, source, arguments));
    }

    private static ParseExpressionFunctionsResult CreateMemberFunctionCallExpression(ExpressionSource source,
        IReadOnlyList<Expression> arguments, MemberAccessLiteralToken memberAccessLiteralToken)
    {
        var path = new IdentifierPath(memberAccessLiteralToken.Parts);
        var expression = new MemberFunctionCallExpression(path, arguments, source);
        return ParseExpressionFunctionsResult.Success(expression);
    }

    private static LexyFunctionCallExpression CreateLexyFunctionCallExpression(string functionName, ExpressionSource source, IReadOnlyList<Expression> arguments)
    {
        return new LexyFunctionCallExpression(functionName, arguments, source);
    }

    private static Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult> Create(
        Func<ExpressionSource, Expression, FunctionCallExpression> factory)
    {
        return (reference, arguments) =>
        {
            if (arguments.Count != 1)
            {
                return ParseExpressionFunctionsResult.Failed("Invalid number of arguments. 1 argument expected.");
            }

            var function = factory(reference, arguments[0]);
            return ParseExpressionFunctionsResult.Success(function);
        };
    }
}