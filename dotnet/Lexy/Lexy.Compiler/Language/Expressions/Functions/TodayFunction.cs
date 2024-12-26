using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class TodayFunction : NoArgumentFunction
    {
        public const string Name = "TODAY";

        protected override VariableType ResultType => PrimitiveType.Date;

        private TodayFunction(SourceReference reference)
            : base(reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference) => new TodayFunction(reference);
    }
}