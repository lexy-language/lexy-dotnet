using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class MonthsFunction : EndStartDateFunction
    {
        public const string Name = "MONTHS";

        protected override string FunctionName => Name;

        private MonthsFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new MonthsFunction(endDateExpression, startDateExpression, reference);
    }
}