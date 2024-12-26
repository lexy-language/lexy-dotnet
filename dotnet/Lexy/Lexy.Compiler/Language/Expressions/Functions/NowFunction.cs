using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public class NowFunction : NoArgumentFunction
    {
        public const string Name = "NOW";

        protected override VariableType ResultType => PrimitiveType.Date;

        private NowFunction(SourceReference reference)
            : base(reference)
        {
        }

        public static BuiltInFunction Create(SourceReference reference) => new NowFunction(reference);
    }
}