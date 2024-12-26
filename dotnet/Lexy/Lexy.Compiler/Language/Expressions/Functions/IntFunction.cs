using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class IntFunction : SingleArgumentFunction
    {
        public const string Name = "INT";

        protected override string FunctionHelp => $"{Name} expects 1 arguments (Value)";

        protected override VariableType ArgumentType => PrimitiveType.Number;
        protected override VariableType ResultType => PrimitiveType.Number;

        private IntFunction(Expression valueExpression, SourceReference reference)
            : base(valueExpression, reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference, Expression expression) =>
            new IntFunction(expression, reference);
    }
}