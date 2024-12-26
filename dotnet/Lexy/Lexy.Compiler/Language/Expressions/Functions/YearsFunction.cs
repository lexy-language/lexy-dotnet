using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class YearsFunction : EndStartDateFunction
    {
        public const string Name = "YEARS";

        protected override string FunctionName => Name;

        private YearsFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new YearsFunction(endDateExpression, startDateExpression, reference);
    }
}