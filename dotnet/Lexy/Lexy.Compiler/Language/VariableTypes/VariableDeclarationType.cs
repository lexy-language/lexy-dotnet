using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class VariableDeclarationType : Node
{
    public VariableType VariableType { get; protected set; }

    protected VariableDeclarationType(SourceReference reference) : base(reference)
    {
    }


    public abstract VariableType CreateVariableType(IValidationContext context);
}