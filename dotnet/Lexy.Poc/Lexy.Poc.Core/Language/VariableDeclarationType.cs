
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class VariableDeclarationType
    {
        public static VariableDeclarationType Parse(string type)
        {
            if (TypeNames.Contains(type))
            {
                return new PrimitiveVariableDeclarationType(type);
            }

            return new CustomVariableDeclarationType(type);
        }

        public abstract VariableType CreateVariableType(IValidationContext context);
    }
}