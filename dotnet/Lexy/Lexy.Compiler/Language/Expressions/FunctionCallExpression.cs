using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; }

        public List<Expression> Arguments { get; }
        public BuiltInFunction BuiltInFunction { get; }

        private FunctionCallExpression(string functionName, List<Expression> arguments,
            BuiltInFunction builtInFunction,
            ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            Arguments = arguments;
            BuiltInFunction = builtInFunction;
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
            var argumentsTokenList = ArgumentList.Parse(innerExpressionTokens);
            if (!argumentsTokenList.IsSuccess)
            {
                return ParseExpressionResult.Invalid<FunctionCallExpression>(argumentsTokenList.ErrorMessage);
            }

            var arguments = new List<Expression>();
            foreach (var argumentTokens in argumentsTokenList.Result)
            {
                var argumentExpression = ExpressionFactory.Parse(source.File, argumentTokens, source.Line);
                if (argumentExpression.Status == ParseExpressionStatus.Failed) return argumentExpression;

                arguments.Add(argumentExpression.Expression);
            }

            var reference = source.CreateReference();

            var builtInFunctionResult = BuiltInFunctions.Parse(functionName, source.CreateReference(), arguments);
            if (!builtInFunctionResult.IsSuccess)
            {
                return ParseExpressionResult.Invalid<FunctionCallExpression>(builtInFunctionResult.ErrorMessage);
            }

            var expression = new FunctionCallExpression(functionName, arguments, builtInFunctionResult.Result, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.IsTokenType<StringLiteralToken>(0)
                   && tokens.OperatorToken(1, OperatorType.OpenParentheses);
        }

        public override IEnumerable<INode> GetChildren()
        {
            if (BuiltInFunction != null)
            {
                yield return BuiltInFunction;
            }
        }

        protected override void Validate(IValidationContext context)
        {
            if (BuiltInFunction == null)
            {
                context.Logger.Fail(Reference, $"Invalid function name: '{FunctionName}'");
                return;
            }
        }

        public override VariableType DeriveType(IValidationContext context)
        {
            return BuiltInFunction?.DeriveReturnType(context);
        }
    }
}