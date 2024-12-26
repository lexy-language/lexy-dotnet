using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class DaysFunction : EndStartDateFunction
    {
        public const string Name = "DAYS";

        protected override string FunctionName => Name;

        private DaysFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new DaysFunction(endDateExpression, startDateExpression, reference);
    }
}