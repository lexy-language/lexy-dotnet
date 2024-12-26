using System.Linq;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions
{
    public abstract class ExpressionFunction : Node
    {
        protected ExpressionFunction(SourceReference reference) : base(reference)
        {
        }

        public abstract VariableType DeriveReturnType(IValidationContext context);
    }
}