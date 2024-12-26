using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class AssignmentExpression : Expression
    {
        public string VariableName { get; }
        public Expression Assignment { get; }

        private AssignmentExpression(string variableName, Expression assignment, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            VariableName = variableName;
            Assignment = assignment;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");
            }

            var variableName = tokens.TokenValue(0);
            var assignment = ExpressionFactory.Parse(source.File, tokens.TokensFrom(2), source.Line);
            if (assignment.Status == ParseExpressionStatus.Failed) return assignment;

            var reference = source.CreateReference();

            var expression = new AssignmentExpression(variableName, assignment.Expression, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.Length >= 3
                   && tokens.IsTokenType<StringLiteralToken>(0)
                   && tokens.OperatorToken(1, OperatorType.Assignment);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Assignment;
        }

        protected override void Validate(IValidationContext context)
        {
            if (!context.FunctionCodeContext.Contains(VariableName))
            {
                context.Logger.Fail(Reference, $"Unknown variable name: '{VariableName}'");
                return;
            }

            var variableType = context.FunctionCodeContext.GetVariableType(VariableName);
            var expressionType = Assignment.DeriveType(context);

            if (variableType == null || !variableType.Equals(expressionType))
            {
                context.Logger.Fail(Reference, $"Variable '{VariableName}' of type '{variableType}' is not assignable from expression of type '{expressionType}'.");
            }
        }

        public override VariableType DeriveType(IValidationContext context) => Assignment.DeriveType(context);
    }
}