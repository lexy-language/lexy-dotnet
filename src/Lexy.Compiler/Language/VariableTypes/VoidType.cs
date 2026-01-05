namespace Lexy.Compiler.Language.VariableTypes;

public class VoidType : VariableType
{
    public override bool IsAssignableFrom(VariableType type) => Equals(type);
}