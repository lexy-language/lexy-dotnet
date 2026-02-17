using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Tests.Specifications;

internal class ExpectedNull : IExpectedSymbol
{
    private readonly int lineNumber;
    private readonly int[] columns;

    private ExpectedNull(int lineNumber, int[] columns)
    {
        this.lineNumber = lineNumber;
        this.columns = columns;
    }

    public static IExpectedSymbol Parse(IReadOnlyList<Token> tokens)
    {
        var lineNumber = Number(tokens[0]);
        var columns = tokens.Skip(2).Select(Number).ToArray();

        return new ExpectedNull(lineNumber, columns);
    }

    private static int Number(Token token)
    {
        if (token is not NumberLiteralToken numberLiteral || numberLiteral.IsDecimal())
        {
            throw new InvalidOperationException("Invalid number: " + token);
        }
        return (int) numberLiteral.NumberValue;
    }

    public bool Verify(IDocumentSymbols symbols, VerifyContext context)
    {
        var failed = false;
        foreach (var column in columns)
        {
            var symbolDescription = symbols.GetDescription(new Position(lineNumber, column));
            context.IsNull(symbolDescription, $"Symbol at {lineNumber}:{column}");

            if (symbolDescription != null)
            {
                failed = true;
            }
        }

        return !failed;
    }
}
