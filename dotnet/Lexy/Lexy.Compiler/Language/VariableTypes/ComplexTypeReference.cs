using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class ComplexTypeReference : VariableType, ITypeWithMembers
{
    public string Name { get; }

    protected ComplexTypeReference(string name)
    {
        Name = name;
    }

    public abstract VariableType MemberType(string name, IValidationContext context);

    public abstract ComplexType GetComplexType(IValidationContext context);
}