using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public abstract class ComplexTypeReference : VariableType, ITypeWithMembers
    {
        protected ComplexTypeReference(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract ComplexType GetComplexType(IValidationContext context);
        public abstract VariableType MemberType(string name, IValidationContext context);
    }
}