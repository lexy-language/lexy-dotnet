using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class IfExpression : Expression, IParsableNode
    {
        private readonly ExpressionList trueExpressions = new ExpressionList();

        public Expression Condition { get; }
        public IEnumerable<Expression> TrueExpressions => trueExpressions;

        public ElseExpression Else { get; set; }

        private IfExpression(Expression condition, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            Condition = condition;
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

            var reference = source.CreateReference();

            var expression = new IfExpression(conditionExpression, source, reference);

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
            if (expression != null)
            {
                trueExpressions.Add(expression, context);
            }
            return expression is IParsableNode node ? node : this;
        }

        protected override IEnumerable<INode> GetChildren()
        {
            yield return Condition;
            foreach (var expression in TrueExpressions)
            {
                yield return expression;
            }
        }

        protected override void Validate(IValidationContext context)
        {
            var type = Condition.DeriveType(context);
            if (type == null || !type.Equals(PrimitiveType.Boolean))
            {
                context.Logger.Fail(Reference,
                    $"'If' condition expression should be 'boolean', is of wrong type '{type}'.");
            }
        }

        internal void LinkElse(ElseExpression elseExpression)
        {
            if (Else != null) throw new InvalidOperationException("'Else' already linked.");

            Else = elseExpression;
        }

        public override VariableType DeriveType(IValidationContext context) => null;
    }
}