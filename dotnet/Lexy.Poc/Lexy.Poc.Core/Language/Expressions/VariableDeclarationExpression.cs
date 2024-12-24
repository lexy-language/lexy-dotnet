using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class VariableDeclarationExpression : Expression
    {
        public VariableDeclarationType Type { get; }
        public string Name { get; }
        public Expression Assignment { get; }

        private VariableDeclarationExpression(VariableDeclarationType variableType, string variableName, Expression assignment,
            ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            Type = variableType ?? throw new ArgumentNullException(nameof(variableType));
            Name = variableName ?? throw new ArgumentNullException(nameof(variableName));
            Assignment = assignment;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<VariableDeclarationExpression>("Invalid expression.");
            }

            var type = VariableDeclarationType.Parse(tokens.TokenValue(0));
            var name = tokens.TokenValue(1);
            var assignment = tokens.Length > 3 ? ExpressionFactory.Parse(source.File, tokens.TokensFrom(3), source.Line) : null;
            var reference = source.CreateReference();

            var expression = new VariableDeclarationExpression(type, name, assignment, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.Length == 2
                   && tokens.IsTokenType<StringLiteralToken>(0)
                   && tokens.IsTokenType<StringLiteralToken>(1)
                   || tokens.Length >= 4
                   && tokens.IsTokenType<StringLiteralToken>(0)
                   && tokens.IsTokenType<StringLiteralToken>(1)
                   && tokens.OperatorToken(2, OperatorType.Assignment);
        }

        protected override IEnumerable<INode> GetChildren()
        {
            if (Assignment != null)
            {
                yield return Assignment;
            }
        }

        protected override void Validate(IValidationContext context)
        {
            var variableType = Type.CreateVariableType(context);
            context.FunctionCodeContext.RegisterVariableAndVerifyUnique(Reference, Name, variableType);

            var memberAccessExpression = Assignment as MemberAccessExpression;
            var literalExpression = Assignment as LiteralExpression;
            if (Assignment != null && literalExpression == null && memberAccessExpression == null)
            {
                context.Logger.Fail(Reference, "Invalid expression. Literal or enum value expression expected.");
            }

            var defaultValue = literalExpression?.Literal;

            context.ValidateTypeAndDefault(Reference, Type, defaultValue);
        }

        public override VariableType DeriveType(IValidationContext context) => null;
    }
}