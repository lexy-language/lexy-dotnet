using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public sealed class ImplicitTypeDeclaration : TypeDeclaration
{
    public ImplicitTypeDeclaration(SourceReference reference) : base(reference)
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
}
