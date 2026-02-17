
namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public abstract class TypeDeclaration : Node
{
    public Type Type { get; protected set; }

    protected TypeDeclaration(NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
    }

    public override string ToString() => Type == null ? "unknown" : Type.ToString();

    public abstract string Label();
}
