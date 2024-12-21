using System.IO;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class Expression
    {
        public Line SourceLine { get; }
        public TokenList Tokens { get; }

        protected Expression(Line sourceLine, TokenList tokens)
        {
            SourceLine = sourceLine;
            Tokens = tokens;
        }

        public static Expression Parse(IParserContext context, TokenList tokens)
        {
            var sourceLine = context.CurrentLine;

            if (tokens.Length >= 3
                && tokens.IsTokenType<StringLiteralToken>(0)
                && tokens.IsTokenType<OperatorToken>(1)
                && tokens.Token<OperatorToken>(1).Type == OperatorType.Assignment)
            {
                return AssignmentExpression.Parse(context, sourceLine, tokens);
            }

            var expression = new Expression(sourceLine, tokens);
            return expression;
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            foreach (var token in Tokens)
            {
                writer.Write(token.Value);
            }
            return writer.ToString();
        }
    }

    public class AssignmentExpression : Expression
    {
        public string VariableName { get; }
        public Expression Expression { get; }

        private AssignmentExpression(Line sourceLine, TokenList tokens, string variableName, Expression expression) : base(sourceLine, tokens)
        {
            VariableName = variableName;
            Expression = expression;
        }

        public static Expression Parse(IParserContext context, Line sourceLine, TokenList tokens)
        {
            if (tokens.Length < 3
                || !tokens.IsTokenType<StringLiteralToken>(0)
                || !tokens.IsTokenType<OperatorToken>(1)
                || tokens.Token<OperatorToken>(1).Type != OperatorType.Assignment)
            {
                context.Logger.Fail("Invalid AssignmentExpression.");
                return null;
            }

            var variableName = tokens.TokenValue(0);
            var expression = Expression.Parse( context, tokens.TokensFrom(2));

            return new AssignmentExpression(sourceLine, tokens, variableName, expression);
        }
    }
}