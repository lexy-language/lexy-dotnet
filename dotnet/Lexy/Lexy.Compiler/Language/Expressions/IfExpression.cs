using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class IfExpression : Expression, IParsableNode
    {
        private readonly ExpressionList trueExpressions;

        public Expression Condition { get; }
        public IEnumerable<Expression> TrueExpressions => trueExpressions;

        public ElseExpression Else { get; set; }

        private IfExpression(Expression condition, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            Condition = condition;
            trueExpressions = new ExpressionList(reference);
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<IfExpression>("Not valid.");
            }

            if (tokens.Length == 1)
            {
                return ParseExpressionResult.Invalid<IfExpression>("No condition found");
            }

            var condition = tokens.TokensFrom(1);
            var conditionExpression = ExpressionFactory.Parse(source.File, condition, source.Line);
            if (conditionExpression.Status == ParseExpressionStatus.Failed) return conditionExpression;

            var reference = source.CreateReference();

            var expression = new IfExpression(conditionExpression.Expression, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.IsKeyword(0, Keywords.If);
        }

        public IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsComment() || line.IsEmpty())
            {
                return this;
            }

            var expression = ExpressionFactory.Parse(context.SourceCode.File, line.Tokens, line);
            if (expression.Status == ParseExpressionStatus.Failed)
            {
                context.Logger.Fail(context.LineStartReference(), expression.ErrorMessage);
                return null;
            }

            trueExpressions.Add(expression.Expression, context);
            return expression.Expression is IParsableNode node ? node : this;
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Condition;
            yield return trueExpressions;
            if (Else != null)
            {
                yield return Else;
            }
        }

        protected override void Validate(IValidationContext context)
        {
            var type = Condition.DeriveType(context);
            if (type == null || !type.Equals(PrimitiveType.Boolean))
            {
                context.Logger.Fail(Reference,
                    $"'if' condition expression should be 'boolean', is of wrong type '{type}'.");
            }
        }

        internal void LinkElse(ElseExpression elseExpression)
        {
            if (Else != null) throw new InvalidOperationException("'else' already linked.");

            Else = elseExpression;
        }

        public override VariableType DeriveType(IValidationContext context) => null;
    }
}