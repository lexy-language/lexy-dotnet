using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public static class ExpressionTestExtensions
{
    public static void ValidateOfType<T>(this object value, Action<T> validate) where T : class
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value is not T specificValue)
        {
            throw new InvalidOperationException(
                $"Value '{value.GetType().Name}' should be of type '{typeof(T).Name}'");
        }

        validate(specificValue);
    }

    public static void ValidateVariableExpression(this Expression expression, string name)
    {
        expression.ValidateOfType<IdentifierExpression>(left =>
            left.Identifier.ShouldBe(name));
    }

    public static void ValidateNumericLiteralExpression(this Expression expression, decimal value)
    {
        expression.ValidateOfType<LiteralExpression>(literal =>
        {
            literal.Literal.ValidateOfType<NumberLiteralToken>(number =>
                number.NumberValue.ShouldBe(value));
        });
    }

    public static void ValidateQuotedLiteralExpression(this Expression expression, string value)
    {
        expression.ValidateOfType<LiteralExpression>(literal =>
        {
            literal.Literal.ValidateOfType<QuotedLiteralToken>(number =>
                number.Value.ShouldBe(value));
        });
    }

    public static void ValidateBooleanLiteralExpression(this Expression expression, bool value)
    {
        expression.ValidateOfType<LiteralExpression>(literal =>
        {
            literal.Literal.ValidateOfType<BooleanLiteral>(number =>
                number.BooleanValue.ShouldBe(value));
        });
    }

    public static void ValidateDateTimeLiteralExpression(this Expression expression, DateTime value)
    {
        expression.ValidateOfType<LiteralExpression>(literal =>
        {
            literal.Literal.ValidateOfType<DateTimeLiteral>(number =>
                number.DateTimeValue.ShouldBe(value));
        });
    }

    public static void ValidateDateTimeLiteralExpression(this Expression expression, string value)
    {
        var valueDate = DateTime.Parse(value);
        expression.ValidateOfType<LiteralExpression>(literal =>
        {
            literal.Literal.ValidateOfType<DateTimeLiteral>(number =>
                number.DateTimeValue.ShouldBe(valueDate));
        });
    }

    public static void ValidateIdentifierExpression(this Expression expression, string value)
    {
        expression.ValidateOfType<IdentifierExpression>(literal => { literal.Identifier.ShouldBe(value); });
    }

    public static void ValidateMemberAccessExpression(this Expression expression, string value)
    {
        expression.ValidateOfType<MemberAccessExpression>(literal => { literal.VariablePath.ToString().ShouldBe(value); });
    }
}