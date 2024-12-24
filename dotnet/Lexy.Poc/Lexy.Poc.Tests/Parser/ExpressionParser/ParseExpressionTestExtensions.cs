using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public static class ParseExpressionTestExtensions
    {
        public static Expression ParseExpression(this ScopedServicesTestFixture fixture, string expression)
        {
            var context = fixture.GetService<IParserContext>();
            var tokenizer = fixture.GetService<ITokenizer>();
            var sourceFile = new SourceFile("generated.lexy");
            var line = new Line(0, expression);

            line.Tokenize(tokenizer, context);

            return ExpressionFactory.Parse(sourceFile, line.Tokens, line);
        }

        public static void ParseExpressionExpectException(this ScopedServicesTestFixture fixture,
            string expression,
            string errorMessage)
        {
            TestContext.ExpectException(
                () => fixture.ParseExpression(expression),
                errorMessage);
        }
    }
}