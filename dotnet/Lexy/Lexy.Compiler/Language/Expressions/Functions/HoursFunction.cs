using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class HoursFunction : EndStartDateFunction
    {
        public const string Name = "HOURS";

        protected override string FunctionName => Name;

        private HoursFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new HoursFunction(endDateExpression, startDateExpression, reference);
    }
}