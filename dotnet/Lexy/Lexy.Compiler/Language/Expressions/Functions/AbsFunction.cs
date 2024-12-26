using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class AbsFunction : SingleArgumentFunction
    {
        public const string Name = "ABS";

        protected override string FunctionHelp => $"{Name} expects 1 arguments (Value)";

        protected override VariableType ArgumentType => PrimitiveType.Number;
        protected override VariableType ResultType => PrimitiveType.Number;

        private AbsFunction(Expression valueExpression, SourceReference reference)
            : base(valueExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression expression) =>
            new AbsFunction(expression, reference);
    }
}