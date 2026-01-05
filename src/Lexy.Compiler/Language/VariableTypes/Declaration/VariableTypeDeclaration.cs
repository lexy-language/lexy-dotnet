using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Declaration;

public abstract class VariableTypeDeclaration : Node
{
    public VariableType VariableType { get; protected set; }

    protected VariableTypeDeclaration(SourceReference reference) : base(reference)
    {
    }


    protected abstract VariableType ValidateVariableType(IValidationContext context);

    protected override void Validate(IValidationContext context)
    {
        VariableType = ValidateVariableType(context);
    }
}