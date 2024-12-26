using System;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser.Tokens;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public static class ExpressionTestExtensions
    {
        public static void ValidateOfType<T>(this object value, Action<T> validate) where T : class
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var specificValue = value as T;
            if (specificValue == null)
            {
                throw new InvalidOperationException(
                    $"Value '{value.GetType().Name}' should be of type ‘{typeof(T).Name}'");
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

        public static void ValidateMemberAccessExpression(this Expression expression, string value)
        {
            expression.ValidateOfType<MemberAccessExpression>(literal =>
            {
                literal.Value.ShouldBe(value);
            });
        }
    }
}