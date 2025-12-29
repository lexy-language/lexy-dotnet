using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions;

public class VariableUsage : VariableReference {

    public VariableAccess Access { get; }

    public VariableUsage(IdentifierPath path, VariableType parentVariableType,
        VariableType variableType, VariableSource source, VariableAccess access) :
        base(path, parentVariableType, variableType, source)
    {
        Access = access;
    }

    public static VariableUsage Read(VariableReference reference)
    {
        return new VariableUsage(reference.Path, reference.ComponentType, reference.VariableType, reference.Source, VariableAccess.Read);
    }

    public static VariableUsage Write(VariableReference reference)
    {
        return new VariableUsage(reference.Path, reference.ComponentType, reference.VariableType, reference.Source, VariableAccess.Write);
    }
}