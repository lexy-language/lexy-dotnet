using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class DayFunction : SingleArgumentFunction
    {
        public const string Name = "DAY";

        protected override string FunctionHelp => $"'{Name} expects 1 arguments (Date)";

        protected override VariableType ArgumentType => PrimitiveType.Date;
        protected override VariableType ResultType => PrimitiveType.Number;

        private DayFunction(Expression valueExpression, SourceReference reference)
            : base(valueExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression expression) =>
            new DayFunction(expression, reference);
    }
}