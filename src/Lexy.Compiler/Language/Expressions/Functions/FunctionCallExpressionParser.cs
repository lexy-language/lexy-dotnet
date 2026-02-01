using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

public static class FunctionCallExpressionParser
{
    private static readonly IDictionary<string, Func<Expression, NodeReference, ExpressionSource, FunctionCallExpression>>
        SystemFunctions = new Dictionary<string, Func<Expression, NodeReference, ExpressionSource, FunctionCallExpression>>
        {
            { NewFunctionExpression.FunctionName, NewFunctionExpression.Create },
            { FillParametersFunctionExpression.FunctionName, FillParametersFunctionExpression.Create },
            { ExtractResultsFunctionExpression.FunctionName, ExtractResultsFunctionExpression.Create }
        };

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
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

        var functionCallReference = new NodeReference();
        var functionNameToken = tokens[0];
        var argumentsTokenListResult = GetArgumentTokens(functionCallReference, source, factory, tokens, matchingClosingParenthesis);
        if (!argumentsTokenListResult.IsSuccess)
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>(argumentsTokenListResult.ErrorMessage);
        }

        var functionCall = Parse(functionNameToken, source, parentReference, argumentsTokenListResult.Result);
        if (!functionCall.IsSuccess)
        {
            return ParseExpressionResult.Invalid<FunctionCallExpression>(functionCall.ErrorMessage);
        }

        functionCallReference.SetNode(functionCall.Result);

        return ParseExpressionResult.Success(functionCall?.Result);
    }

    private static ParseExpressionsResult GetArgumentTokens(
        NodeReference functionCallReference,
        ExpressionSource source, IExpressionFactory factory,
        TokenList tokens, int matchingClosingParenthesis)
    {
        var innerExpressionTokens = tokens.TokensRange(2, matchingClosingParenthesis - 1);
        var argumentsTokenList = ArgumentList.Parse(innerExpressionTokens);
        if (!argumentsTokenList.IsSuccess)
        {
            return ParseExpressionsResult.Invalid<FunctionCallExpression>(argumentsTokenList.ErrorMessage);
        }

        var arguments = new List<Expression>();
        foreach (var argumentTokens in argumentsTokenList.Result)
        {
            var argumentExpression = factory.Parse(functionCallReference, argumentTokens, source.Line);
            if (!argumentExpression.IsSuccess)
            {
                return ParseExpressionsResult.Invalid<FunctionCallExpression>(argumentExpression.ErrorMessage);
            }

            arguments.Add(argumentExpression.Result);
        }

        return ParseExpressionsResult.Success(arguments);
    }

    private static ParseExpressionFunctionsResult Parse(Token functionNameToken, ExpressionSource source,
        NodeReference parentReference, IReadOnlyList<Expression> arguments)
    {
        return functionNameToken switch
        {
            StringLiteralToken stringLiteralToken =>
                ParseStringLiteralFunctionCall(stringLiteralToken, arguments, parentReference, source),

            MemberAccessToken memberAccessToken =>
                CreateMemberFunctionCallExpression(memberAccessToken, arguments, parentReference, source),

            _ => throw new InvalidOperationException($"Invalid token type: {functionNameToken.GetType()}")
        };
    }

    private static ParseExpressionFunctionsResult ParseStringLiteralFunctionCall(StringLiteralToken stringLiteralToken,
        IReadOnlyList<Expression> arguments, NodeReference parentReference, ExpressionSource source)
    {
        var functionName = stringLiteralToken.Value;
        if (SystemFunctions.TryGetValue(functionName, out var value))
        {
            return ParseSystemFunctionCall(arguments, parentReference, source, value);
        }

        var expression = CreateLexyFunctionCallExpression(functionName, arguments, parentReference, source);
        return ParseExpressionFunctionsResult.Success(expression);
    }

    private static ParseExpressionFunctionsResult ParseSystemFunctionCall(IReadOnlyList<Expression> arguments,
        NodeReference parentReference, ExpressionSource source, Func<Expression, NodeReference, ExpressionSource, FunctionCallExpression> value)
    {
        if (arguments.Count != 1)
        {
            return ParseExpressionFunctionsResult.Failed("Invalid number of arguments. 1 argument expected.");
        }

        var expression = value(arguments[0], parentReference, source);
        return ParseExpressionFunctionsResult.Success(expression);
    }

    private static ParseExpressionFunctionsResult CreateMemberFunctionCallExpression(MemberAccessToken memberAccessLiteralToken, IReadOnlyList<Expression> arguments,
        NodeReference parentReference, ExpressionSource source)
    {
        var path = new IdentifierPath(memberAccessLiteralToken.Parts);
        var expression = new MemberFunctionCallExpression(path, arguments, parentReference, source);
        return ParseExpressionFunctionsResult.Success(expression);
    }

    private static LexyFunctionCallExpression CreateLexyFunctionCallExpression(string functionName, IReadOnlyList<Expression> arguments, NodeReference parentReference, ExpressionSource source)
    {
        return new LexyFunctionCallExpression(functionName, arguments, parentReference, source);
    }

    private static Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult> ForFirstArgument(
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
