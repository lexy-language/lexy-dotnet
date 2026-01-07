using System;
using System.Text;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class OperatorsTests : ScopedServicesTestFixture
{
    private record OperatorEntry(string Value, OperatorType Type);

    private readonly OperatorEntry[] operators = {
        new ("=", OperatorType.Assignment),
        new ("+", OperatorType.Addition),
        new ("-", OperatorType.Subtraction),
        new ("*", OperatorType.Multiplication),
        new ("/", OperatorType.Division),
        new ("%", OperatorType.Modulus),
        new ("(", OperatorType.OpenParentheses),
        new (")", OperatorType.CloseParentheses),
        new ("[", OperatorType.OpenBrackets),
        new ("]", OperatorType.CloseBrackets),
        new (">", OperatorType.GreaterThan),
        new ("<", OperatorType.LessThan),
        new (">=", OperatorType.GreaterThanOrEqual),
        new ("<=", OperatorType.LessThanOrEqual),
        new ("==", OperatorType.Equals),
        new ("!=", OperatorType.NotEqual),
        new ("&&", OperatorType.And),
        new ("||", OperatorType.Or),
        new (",", OperatorType.ArgumentSeparator),
        new ("...", OperatorType.Spread),
    };

    [Test]
    public void TestOperatorAtEndOfLineTokens()
    {
        var builder = new StringBuilder();
        operators.ForEach(entry => ValidateOperatorToken(entry, builder));

        if (builder.Length > 0)
        {
            throw new InvalidOperationException(builder.ToString());
        }
    }

    [Test]
    public void TestOperatorWithWhitespaceSuffixTokens()
    {
        var builder = new StringBuilder();
        operators.ForEach(entry => ValidateOperatorToken(entry, builder, value => value += " "));

        if (builder.Length > 0)
        {
            throw new InvalidOperationException(builder.ToString());
        }
    }

    private void ValidateOperatorToken(OperatorEntry operatorEntry, StringBuilder errors, Func<string, string> valueModifier = null)
    {
        var value = valueModifier != null ? valueModifier(operatorEntry.Value) : operatorEntry.Value;
        try
        {
            ServiceProvider
                .Tokenize(value)
                .Count(1)
                .Operator(0, operatorEntry.Type)
                .Assert();
        }
        catch (Exception exception)
        {
            errors.AppendLine("'" + operatorEntry.Value + "' (" + operatorEntry.Type + "): " + exception.Message);
        }
    }
}