using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; }
        public Expression Expression { get; }

        private FunctionCallExpression(string functionName, Expression expression,
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
                return ParseExpressionResult.Invalid<FunctionCallExpression>("Not valid.");
            }

            var matchingClosingParenthesis = ParenthesizedExpression.FindMatchingClosingParenthesis(tokens);
            if (matchingClosingParenthesis == -1)
            {
                return ParseExpressionResult.Invalid<FunctionCallExpression>("No closing parentheses found.");
            }

            var functionName = tokens.TokenValue(0);
            var innerExpressionTokens = tokens.TokensRange(2, matchingClosingParenthesis - 1);
            var innerExpression = ExpressionFactory.Parse(source.File, innerExpressionTokens, source.Line);
            var reference = source.CreateReference();

            var expression = new FunctionCallExpression(functionName, innerExpression, source, reference);
            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.IsTokenType<StringLiteralToken>(0)
                   && tokens.OperatorToken(1, OperatorType.OpenParentheses);
        }

        protected override IEnumerable<INode> GetChildren()
        {
            yield return Expression;
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public override VariableType DeriveType(IValidationContext context)
        {
            throw new InvalidOperationException("Not yet supported/");
        }
    }
}