using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public abstract class ComplexTypeType : VariableType
    {
        protected ComplexTypeType(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract ComplexType GetComplexType(IValidationContext context);
    }
}