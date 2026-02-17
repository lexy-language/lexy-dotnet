using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Tests.Specifications;

internal class ExpectedSymbol : IExpectedSymbol
{
    private readonly int lineNumber;
    private readonly int column;
    private readonly string name;
    private readonly SymbolKind kind;
    private readonly string description;

    private ExpectedSymbol(int lineNumber, int column, string name, SymbolKind kind, string description)
    {
        this.lineNumber = lineNumber;
        this.column = column;
        this.name = name;
        this.kind = kind;
        this.description = description?.Replace("\\n", "\n");
    }

    public static IExpectedSymbol Parse(string line)
    {
        var tokenizer = new Lexy.Compiler.Parser.Tokens.Tokenizer();
        var tokenizeResult = tokenizer.Tokenize(new Line(0, line, TestFile.Instance));
        var parts = GetTokens(line, tokenizeResult);
        if (parts.Count <= 1) return null;

        if (parts[1].Value.Trim() == "null")
        {
            return ExpectedNull.Parse(parts);
        }

        if (parts.Count != 4 && parts.Count != 5)
        {
            throw new InvalidOperationException("Invalid values (4 or 5 expected): " + line);
        }

        var lineNumber = Number(parts[0]);
        var column = Number(parts[1]);
        var name = Trim(parts[2]);
        var kind = ParseSymbolKind(parts[3]);
        var description = parts.Count > 4 ? Trim(parts[4]) : null;

        return new ExpectedSymbol(lineNumber, column, name, kind, description);
    }

    private static SymbolKind ParseSymbolKind(Token token)
    {
        if (token is not MemberAccessToken { Parent: "SymbolKind" } enumMember)
        {
            throw new InvalidOperationException("Invalid SymbolKind: " + token);
        }
        return Enum.Parse<SymbolKind>(enumMember.Member);
    }

    private static int Number(Token token)
    {
        if (token is not NumberLiteralToken numberLiteral || numberLiteral.IsDecimal())
        {
            throw new InvalidOperationException("Invalid number: " + token);
        }
        return (int) numberLiteral.NumberValue;
    }

    private static IReadOnlyList<Token> GetTokens(string line, TokenizeResult tokenizeResult)
    {
        if (!tokenizeResult.IsSuccess)
        {
            throw new InvalidOperationException("Invalid line: " + line);
        }

        var result = new List<Token>();
        var tokens = tokenizeResult.Result;
        for (var index = 0; index < tokens.Length; index += 2)
        {
            if (index < tokens.Length - 1 && !IsComma(tokens, index + 1))
            {
                throw new InvalidOperationException("Comma expected at : " + tokens[index + 1].FirstCharacter.Position);
            }
            result.Add(tokens[index]);
        }
        return result;
    }

    private static bool IsComma(TokenList tokens, int index)
    {
        return tokens[index] is OperatorToken operatorToken && operatorToken.Type == OperatorType.ArgumentSeparator;
    }

    private static string Trim(Token token) => token.Value;

    public bool Verify(IDocumentSymbols symbols, VerifyContext context)
    {
        var extraMessage = $"Symbol at ({lineNumber}:{column})";
        var position = new Position(lineNumber, column);
        var symbolDescription = symbols.GetDescription(position);

        context.IsNotNull(symbolDescription, symbolContext => symbolContext
            .AreEqual(symbol => symbol.Name, name, extraMessage)
            .AreEqual(symbol => symbol.Kind, kind, extraMessage)
            .IfNotNull(description, descriptionContext => descriptionContext
                .AreEqual(symbol => symbol.Description, description, extraMessage)
            ),
            extraMessage
        );

        return symbolDescription != null
            && symbolDescription.Name == name
            && symbolDescription.Kind == kind
            && (description == null || symbolDescription.Description == description);
    }
}
