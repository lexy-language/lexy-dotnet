using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

public class VariableUsage : VariableReference {

    public VariableAccess Access { get; }

    public VariableUsage(SourceReference reference, IdentifierPath path, Type parentType,
        Type type, VariableSource source, VariableAccess access) :
        base(reference, path, parentType, type, source)
    {
        Access = access;
    }

    public static VariableUsage Read(VariableReference reference)
    {
        return new VariableUsage(reference.Reference, reference.Path, reference.ComponentType, reference.Type, reference.Source, VariableAccess.Read);
    }

    public static VariableUsage Write(VariableReference reference)
    {
        return new VariableUsage(reference.Reference, reference.Path, reference.ComponentType, reference.Type, reference.Source, VariableAccess.Write);
    }
}
