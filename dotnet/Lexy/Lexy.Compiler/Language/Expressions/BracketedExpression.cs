using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class BracketedExpression : Expression
    {
        public string FunctionName { get; }
        public Expression Expression { get; }

        private BracketedExpression(string functionName, Expression expression,
            ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            FunctionName = functionName;
            Expression = expression;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<BracketedExpression>("Not valid.");
            }

            var matchingClosingParenthesis = FindMatchingClosingBracket(tokens);
            if (matchingClosingParenthesis == -1)
            {
                return ParseExpressionResult.Invalid<BracketedExpression>("No closing bracket found.");
            }

            var functionName = tokens.TokenValue(0);
            var innerExpressionTokens = tokens.TokensRange(2, matchingClosingParenthesis - 1);
            var innerExpression = ExpressionFactory.Parse(source.File, innerExpressionTokens, source.Line);
            if (innerExpression.Status == ParseExpressionStatus.Failed) return innerExpression;

            var reference = source.CreateReference();

            var expression = new BracketedExpression(functionName, innerExpression.Expression, source, reference);
            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.Length > 1
                && tokens.IsTokenType<StringLiteralToken>(0)
                && tokens.OperatorToken(1, OperatorType.OpenBrackets);
        }

        internal static int FindMatchingClosingBracket(TokenList tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            var count = 0;
            for (var index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];
                if (!(token is OperatorToken operatorToken)) continue;

                if (operatorToken.Type == OperatorType.OpenBrackets)
                {
                    count++;
                }
                else if (operatorToken.Type == OperatorType.CloseBrackets)
                {
                    count--;
                    if (count == 0)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Expression;
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public override VariableType DeriveType(IValidationContext context) => Expression.DeriveType(context);
    }
}