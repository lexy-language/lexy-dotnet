using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class SwitchExpression : Expression, IParsableNode
    {
        private readonly IList<CaseExpression> cases = new List<CaseExpression>();

        public Expression Condition { get; }
        public IEnumerable<CaseExpression> Cases => cases;

        private SwitchExpression(Expression condition, ExpressionSource source, SourceReference reference) : base(source, reference)
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

            var expression = new SwitchExpression(conditionExpression, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.IsKeyword(0, Keywords.Switch);
        }

        public IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsComment() || line.IsEmpty())
            {
                return this;
            }

            var expression = ExpressionFactory.Parse(context.SourceCode.File, line.Tokens, line) ;
            if (expression is CaseExpression caseExpression)
            {
                caseExpression.LinkPreviousExpression(this, context);
                return caseExpression;
            }

            context.Logger.Fail(expression.Reference, "Invalid expression. 'case' or 'default' expected.");
            return this;
        }

        protected override IEnumerable<INode> GetChildren()
        {
            yield return Condition;
            foreach (var caseValue in Cases)
            {
                yield return caseValue;
            }
        }

        protected override void Validate(IValidationContext context)
        {
            var type = Condition.DeriveType(context);
            if (type == null
                || !(type is PrimitiveType) && !(type is EnumType))
            {
                context.Logger.Fail(Reference,
                    $"'Switch' condition expression should have a primitive or enum type. Not: '{type}'.");
                return;
            }

            foreach (var caseExpression in cases)
            {
                if (caseExpression.IsDefault) continue;

                var caseType = caseExpression.DeriveType(context);
                if (caseType == null || !type.Equals(caseType))
                {
                    context.Logger.Fail(Reference,
                        $"'case' condition expression should be of type '{type}', is of wrong type '{caseType}'.");
                }
            }
        }

        internal void LinkElse(CaseExpression caseExpression)
        {
            cases.Add(caseExpression);
        }

        public override VariableType DeriveType(IValidationContext context) => null;
    }

    public class CaseExpression : Expression, IParsableNode, IDependantExpression
    {
        private readonly ExpressionList expressions = new ExpressionList();

        public Expression Value { get; }
        public IEnumerable<Expression> Expressions => expressions;
        public bool IsDefault { get; }

        private CaseExpression(Expression value, bool isDefault, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            Value = value;
            IsDefault = isDefault;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<IfExpression>("Not valid.");
            }

            if (tokens.IsKeyword(0, Keywords.Default))
            {
                return ParseDefaultCase(source, tokens);
            }

            if (tokens.Length == 1)
            {
                return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'case'. No parameters found.");
            }

            var value = tokens.TokensFrom(1);
            var valueExpression = ExpressionFactory.Parse(source.File, value, source.Line);

            var reference = source.CreateReference();

            var expression = new CaseExpression(valueExpression, false , source, reference);

            return ParseExpressionResult.Success(expression);
        }

        private static ParseExpressionResult ParseDefaultCase(ExpressionSource source, TokenList tokens)
        {
            if (tokens.Length != 1)
            {
                return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'default' case. No parameters expected.");
            }

            var reference = source.CreateReference();
            var expression = new CaseExpression(null, true, source, reference);
            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.IsKeyword(0, Keywords.Case)
                || tokens.IsKeyword(0, Keywords.Default);
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
                expressions.Add(expression, context);
            }
            return expression is IParsableNode node ? node : this;
        }

        protected override IEnumerable<INode> GetChildren()
        {
            if (Value != null) yield return Value;

            foreach (var expression in Expressions)
            {
                yield return expression;
            }
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public override VariableType DeriveType(IValidationContext context) => Value?.DeriveType(context);

        public void LinkPreviousExpression(Expression expression, IParserContext context)
        {
            if (!(expression is SwitchExpression switchExpression))
            {
                context.Logger.Fail(Reference, "'case' should be following a 'switch' statement. No 'switch' statement found.");
                return;
            }

            switchExpression.LinkElse(this);
        }
    }

}