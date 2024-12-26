using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class HourFunction : SingleArgumentFunction
    {
        public const string Name = "HOUR";

        protected override string FunctionHelp => $"'{Name} expects 1 arguments (Date)";

        protected override VariableType ArgumentType => PrimitiveType.Date;
        protected override VariableType ResultType => PrimitiveType.Number;

        private HourFunction(Expression valueExpression, SourceReference reference)
            : base(valueExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression expression) =>
            new HourFunction(expression, reference);
    }
}