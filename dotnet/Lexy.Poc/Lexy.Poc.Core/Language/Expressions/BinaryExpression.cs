using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class BinaryExpression : Expression
    {
        private static readonly IDictionary<OperatorType, ExpressionOperator> supportedOperators = new Dictionary<OperatorType, ExpressionOperator>
        {
            { OperatorType.Addition, ExpressionOperator.Addition },
            { OperatorType.Subtraction, ExpressionOperator.Subtraction },
            { OperatorType.Multiplication, ExpressionOperator.Multiplication },
            { OperatorType.Division, ExpressionOperator.Division },
            { OperatorType.Modulus, ExpressionOperator.Modulus },

            { OperatorType.GreaterThan, ExpressionOperator.GreaterThan },
            { OperatorType.GreaterThanOrEqual, ExpressionOperator.GreaterThanOrEqual },
            { OperatorType.LessThan, ExpressionOperator.LessThan },
            { OperatorType.LessThanOrEqual, ExpressionOperator.LessThanOrEqual },

            { OperatorType.Equals, ExpressionOperator.Equals },
            { OperatorType.NotEqual, ExpressionOperator.NotEqual },

            { OperatorType.And, ExpressionOperator.And },
            { OperatorType.Or, ExpressionOperator.Or },
        };

        public Expression Left { get; }
        public Expression Right { get; }
        public ExpressionOperator Operator { get; set; }

        private BinaryExpression(Expression left, Expression right, ExpressionOperator operatorValue, Line sourceLine,
            TokenList tokens) : base(sourceLine, tokens)
        {
            Left = left;
            Right = right;
            Operator = operatorValue;
        }

        public static ParseExpressionResult Parse(Line sourceLine, TokenList tokens)
        {
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<BinaryExpression>("Invalid expression.");
            }

            var left = ExpressionFactory.Parse(tokens.TokensFromStart(1), sourceLine);
            var right = ExpressionFactory.Parse(tokens.TokensFrom(2), sourceLine);
            var operatorValue = OperatorValue(tokens.Token<OperatorToken>(1).Type);

            var binaryExpression = new BinaryExpression(left, right, operatorValue, sourceLine, tokens);
            return ParseExpressionResult.Success(binaryExpression) ;
        }

        private static ExpressionOperator OperatorValue(OperatorType operatorType)
        {
            if (supportedOperators.ContainsKey(operatorType))
            {
                return supportedOperators[operatorType];
            }
            throw new ArgumentOutOfRangeException(nameof(operatorType), operatorType, null);
        }

        public static bool IsValid(TokenList tokens)
        {
            if (tokens.Length < 3 || !tokens.IsTokenType<OperatorToken>(1)) return false;

            var operatorToken = tokens.OperatorToken(1);

            return operatorToken != null && supportedOperators.ContainsKey(operatorToken.Type);
        }
    }
}