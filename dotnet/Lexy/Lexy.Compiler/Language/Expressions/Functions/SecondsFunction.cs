using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class SecondsFunction : EndStartDateFunction
    {
        public const string Name = "SECONDS";

        protected override string FunctionName => Name;

        private SecondsFunction(Expression endDateExpression, Expression startDateExpression, SourceReference reference)
            : base(endDateExpression, startDateExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression endDateExpression,
            Expression startDateExpression) =>
            new SecondsFunction(endDateExpression, startDateExpression, reference);
    }
}