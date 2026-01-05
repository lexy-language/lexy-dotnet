using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public static class ArgumentList
{
    private record ParseContext
    {
        public List<TokenList> Result { get; } = new();
        public List<Token> ArgumentTokens { get; } = new();
        public int CountParentheses = 0;
        public int CountBrackets = 0;
    }

    public static ArgumentTokenParseResult Parse(TokenList tokens)
    {
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));
        if (tokens.Length == 0) return ArgumentTokenParseResult.Success(Array.Empty<TokenList>());

        var context = new ParseContext();
        foreach (var token in tokens)
        {
            var result = ProcessToken(context, token);
            if (result != null) return result;
        }

        if (context.ArgumentTokens.Count == 0)
        {
            return ArgumentTokenParseResult.Failed(@"Invalid token ','. No tokens before comma.");
        }

        context.Result.Add(new TokenList(context.ArgumentTokens.ToArray()));

        return ArgumentTokenParseResult.Success(context.Result);
    }

    private static ArgumentTokenParseResult ProcessToken(ParseContext context, Token token)
    {
        if (token is not OperatorToken operatorToken)
        {
            context.ArgumentTokens.Add(token);
            return null;
        }

        CountScopeCharacters(context, operatorToken);

        if (context.CountParentheses == 0
         && context.CountBrackets == 0
         && operatorToken.Type == OperatorType.ArgumentSeparator)
        {
            if (context.ArgumentTokens.Count == 0)
            {
                return ArgumentTokenParseResult.Failed(@"Invalid token ','. No tokens before comma.");
            }

            context.Result.Add(new TokenList(context.ArgumentTokens.ToArray()));
            context.ArgumentTokens.Clear();
        }
        else
        {
            context.ArgumentTokens.Add(token);
        }
        return null;
    }

    private static void CountScopeCharacters(ParseContext context, OperatorToken operatorToken)
    {
        switch (operatorToken.Type)
        {
            case OperatorType.OpenParentheses:
                context.CountParentheses++;
                break;
            case OperatorType.CloseParentheses:
                context.CountParentheses--;
                break;
            case OperatorType.OpenBrackets:
                context.CountBrackets++;
                break;
            case OperatorType.CloseBrackets:
                context.CountBrackets--;
                break;
        }
    }
}