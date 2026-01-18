using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Expressions;

public class VariableUsage : VariableReference {

    public VariableAccess Access { get; }

    public VariableUsage(IdentifierPath path, Type parentType,
        Type type, VariableSource source, VariableAccess access) :
        base(path, parentType, type, source)
    {
        Access = access;
    }

    public static VariableUsage Read(VariableReference reference)
    {
        return new VariableUsage(reference.Path, reference.ComponentType, reference.Type, reference.Source, VariableAccess.Read);
    }

    public static VariableUsage Write(VariableReference reference)
    {
        return new VariableUsage(reference.Path, reference.ComponentType, reference.Type, reference.Source, VariableAccess.Write);
    }
}