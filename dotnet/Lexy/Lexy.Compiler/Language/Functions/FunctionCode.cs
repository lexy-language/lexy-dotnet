using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language
{
    public class FunctionCode : ParsableNode
    {
        private readonly ExpressionList expressions;

        public IReadOnlyList<Expression> Expressions => expressions;

        public FunctionCode(SourceReference reference) : base(reference)
        {
            expressions = new ExpressionList(reference);
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsComment() || line.IsEmpty())
            {
                return this;
            }

            var expression = ExpressionFactory.Parse(context.SourceCode.File, line.Tokens, line);
            if (context.Failed(expression, context.LineStartReference())) return this;

            expressions.Add(expression.Result, context);

            return expression.Result is IParsableNode node ? node : this;
        }

        public override IEnumerable<INode> GetChildren() => Expressions;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}