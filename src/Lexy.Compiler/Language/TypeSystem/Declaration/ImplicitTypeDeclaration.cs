using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public sealed class ImplicitTypeDeclaration : TypeDeclaration
{
    public ImplicitTypeDeclaration(NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
    }

    public void Define(Type type)
    {
        Type = type;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    public override Symbol GetSymbol()
    {
        return Type?.GetSymbol(Reference);
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override string Label() => "var";
}
