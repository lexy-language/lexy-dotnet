using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class MinutesFunction : EndStartDateFunction
    {
        public const string Name = "MINUTES";

        protected override string FunctionName => Name;

        private MinutesFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new MinutesFunction(endDateExpression, startDateExpression, reference);
    }
}